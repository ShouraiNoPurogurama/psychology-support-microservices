using BuildingBlocks.Messaging.Masstransit;
using Carter;
using ChatBox.API.Data;
using ChatBox.API.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace ChatBox.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        
        // services.AddCarter();

        ConfigureSwagger(services);

        ConfigureCors(services);
        
        AddDatabase(services, config);

        AddServiceDependencies(services);
        
        services.AddSignalR();
        
        services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly);
        
        return services;
    }

    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Chatbox API",
                Version = "v1"
            });
            options.AddServer(new OpenApiServer
            {
                Url = "/chatbox-service/"
            });
        });
    }

    private static void ConfigureCors(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {

                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials(); // Enable credentials support
            });
        });
    }

    private static void AddServiceDependencies(IServiceCollection services)
    {
        services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
        // services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        // services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
    }

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