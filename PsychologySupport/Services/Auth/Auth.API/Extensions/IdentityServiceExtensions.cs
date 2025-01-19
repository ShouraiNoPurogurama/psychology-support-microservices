using Auth.API.Data;
using Auth.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<AuthDbContext>();

        services.Configure<IdentityOptions>(options =>
        {
            //Set your desired password requirements here
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 0;

            //Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = false;

            //User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        });

        services.Configure<PasswordHasherOptions>(opt =>
        {
            opt.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
        });
        
        services.AddAuthentication();
        services.AddAuthorization();

        return services;
    }
}