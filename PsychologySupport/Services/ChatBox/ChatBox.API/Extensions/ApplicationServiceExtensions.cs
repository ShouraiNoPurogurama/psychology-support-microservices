using BuildingBlocks.Messaging.Masstransit;
using Carter;
using ChatBox.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace ChatBox.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddCarter();

        ConfigureSwagger(services);

        ConfigureCors(services);

        // ConfigureMediatR(services);

        AddDatabase(services, config);

        // AddServiceDependencies(services);
        
        services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly);
        
        return services;
    }

    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ChatBox API",
            Version = "v1"
        }));
    }

    // private static void ConfigureMediatR(IServiceCollection services)
    // {
    //     services.AddMediatR(configuration =>
    //     {
    //         configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
    //         configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
    //         configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
    //     });
    // }

    private static void ConfigureCors(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                // builder
                //     .AllowAnyOrigin()
                //     .AllowAnyMethod()
                //     .AllowAnyHeader();
                builder.WithOrigins("https://localhost:7152") // Change to your frontend's URL
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials(); // Enable credentials support
            });
        });
    }

    // private static void AddServiceDependencies(IServiceCollection services)
    // {
    //     services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
    //     services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
    // }

    private static void AddDatabase(IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("ChatBoxDb");

        services.AddDbContext<ChatBoxDbContext>((sp, opt) =>
        {
            // opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opt.UseNpgsql(connectionString);
        });

        services.AddScoped<DbContext, ChatBoxDbContext>();
    }
}