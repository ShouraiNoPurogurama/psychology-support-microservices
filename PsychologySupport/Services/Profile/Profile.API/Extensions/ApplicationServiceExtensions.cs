using BuildingBlocks.Messaging.Masstransit;
using Carter;
using FluentValidation;
using Profile.API.DoctorProfiles.Validators;
using Profile.API.PatientProfiles.Validators;

namespace Profile.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddEndpointsApiExplorer();

        services.AddCarter();

        ConfigureSwagger(services);

        ConfigureCors(services);

        ConfigureMediatR(services);

        AddDatabase(services, config);

        AddServiceDependencies(services);

        services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly);

        services.AddValidatorsFromAssemblyContaining<UpdateDoctorProfileValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdatePatientProfileValidator>();

        return services;
    }

    private static void AddServiceDependencies(IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
    }

    private static void ConfigureMediatR(IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
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

    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Profile API",
                Version = "v1"
            });
            options.AddServer(new OpenApiServer
            {
                Url = "/profile-service/"
            });
        });
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("ProfileDb");

        services.AddDbContext<ProfileDbContext>((sp, opt) =>
        {
            opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opt.UseNpgsql(connectionString);
        });
    }
}