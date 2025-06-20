using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ChatBox.API.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureAuthentication(services);
        return services;
    }

    private static void ConfigureAuthentication(IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
    }
}