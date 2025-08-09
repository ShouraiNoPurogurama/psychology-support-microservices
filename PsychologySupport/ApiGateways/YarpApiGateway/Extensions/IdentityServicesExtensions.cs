using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace YarpApiGateway.Extensions;

public static class IdentityServicesExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // options.Authority = config["IdentityServer:Authority"];

                var rsaKey = RSA.Create();
                string xmlKey = File.ReadAllText(config.GetSection("Jwt:PublicKeyPath").Value!);
                rsaKey.FromXmlString(xmlKey);

                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidAudience = config["Jwt:Audience"],
                    ValidIssuer = config["Jwt:Issuer"],
                    IssuerSigningKey = new RsaSecurityKey(rsaKey)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin", policy =>
                policy.RequireClaim(ClaimTypes.Role, "Admin"));
            options.AddPolicy("RequireUser", policy =>
                policy.RequireClaim(ClaimTypes.Role, "User"));
            options.AddPolicy("RequireManager", policy =>
                policy.RequireClaim(ClaimTypes.Role, "Manager"));
            options.AddPolicy("RequireUser", policy =>
                policy.RequireClaim(ClaimTypes.Role, "User"));
            options.AddPolicy("RequireDoctor", policy =>
                policy.RequireClaim(ClaimTypes.Role, "Doctor"));
            options.AddPolicy("AuthenticatedUser", policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }
}