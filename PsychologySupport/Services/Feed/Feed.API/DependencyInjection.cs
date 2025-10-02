using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using Cassandra;
using Feed.API.Extensions;
using Feed.API.HealthChecks;
using Feed.Infrastructure.Data.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using ISession = Cassandra.ISession;

namespace Feed.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config,
        IWebHostEnvironment env)
    {
        services.AddCarter();
        
        services.AddExceptionHandler<CustomExceptionHandler>();

        services.AddHttpContextAccessor();
        
        AddCassandra(services, config);
        
        services.AddHttpContextAccessor();

        services.AddIdentityServices(config);

        services.AddAuthorization();
        
        ConfigureSwagger(services, env);

        ConfigureCors(services);

        // OPTIMIZED: Add health checks for monitoring
        services.AddHealthChecks()
            .AddCheck<CassandraHealthCheck>("cassandra", 
                failureStatus: HealthStatus.Unhealthy, 
                tags: new[] { "db", "cassandra", "ready" })
            .AddCheck<RedisHealthCheck>("redis", 
                failureStatus: HealthStatus.Degraded, 
                tags: new[] { "cache", "redis", "ready" });

        return services;
    }
    
    private static void ConfigureCors(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }

    private static void ConfigureSwagger(IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Feed API",
                Version = "v1"
            });

            var url = env.IsProduction()
                ? "/Feed-service"
                : "https://localhost:5510/feed-service";

            options.AddServer(new Microsoft.OpenApi.Models.OpenApiServer
            {
                Url = url
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.\n\nEnter: **Bearer &lt;your token&gt;**",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    public static IServiceCollection AddCassandra(this IServiceCollection services, IConfiguration config)
    {
        // Bind "ConnectionStrings:Cassandra" vào options
        services.Configure<CassandraOptions>(config.GetSection("ConnectionStrings:Cassandra"));

        // ICluster (singleton)
        services.AddSingleton<ICluster>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<CassandraOptions>>().Value;

            var builder = Cluster.Builder()
                .AddContactPoints(opt.ContactPoints.Length > 0 ? opt.ContactPoints : new[] { "127.0.0.1" })
                .WithPort(opt.Port)
                // OPTIMIZED: Configure connection pooling for better performance
                .WithPoolingOptions(new PoolingOptions()
                    .SetCoreConnectionsPerHost(HostDistance.Local, 4)   // Up from default 2
                    .SetMaxConnectionsPerHost(HostDistance.Local, 8)     // Default is 8, keeping same
                    .SetMaxSimultaneousRequestsPerConnectionTreshold(HostDistance.Local, 1500) // Default 1500
                    .SetHeartBeatInterval(30000)) // 30 seconds heartbeat
                .WithLoadBalancingPolicy(
                    new TokenAwarePolicy(new DCAwareRoundRobinPolicy(opt.LocalDc)))
                .WithQueryOptions(
                    new QueryOptions()
                        .SetConsistencyLevel(ConsistencyLevel.LocalQuorum)
                        .SetDefaultIdempotence(true)
                        .SetPageSize(5000)) // Optimize fetch size
                .WithReconnectionPolicy(new ExponentialReconnectionPolicy(1000, 10000)) // 1s to 10s
                .WithQueryTimeout(10000); // 10 second query timeout

            if (!string.IsNullOrWhiteSpace(opt.Username))
                builder = builder.WithCredentials(opt.Username, opt.Password);

            return builder.Build();
        });

        services.AddSingleton<ISession>(sp =>
        {
            var cluster = sp.GetRequiredService<ICluster>();
            var opt = sp.GetRequiredService<IOptions<CassandraOptions>>().Value;
            EnsureKeyspaceExists(cluster, opt);
            return cluster.Connect(opt.Keyspace);
        });

        return services;
    }

    private static void EnsureKeyspaceExists(ICluster cluster, CassandraOptions opt)
    {
        using var session = cluster.Connect();
        // Dev: SimpleStrategy RF=1; Prod: NetworkTopologyStrategy theo DC
        var cql = $@"
CREATE KEYSPACE IF NOT EXISTS {opt.Keyspace}
WITH REPLICATION = {{ 'class': 'SimpleStrategy', 'replication_factor': 1 }}";
        session.Execute(cql);
    }

    private static string? GetConnectionString(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("FeedDb");
        return connectionString;
    }
}