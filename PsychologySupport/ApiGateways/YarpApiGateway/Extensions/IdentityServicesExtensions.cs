using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YarpApiGateway.Extensions;

public static class IdentityServicesExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = config["IdentityServer:Authority"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false
                };

                var rsaKey = RSA.Create();
                string xmlKey = File.ReadAllText(config.GetSection("Jwt:PublicKeyPath").Value!);
                rsaKey.FromXmlString(xmlKey);
                
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,

                    // IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
                    IssuerSigningKey = new RsaSecurityKey(rsaKey)
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        var payload = new JObject
                        {
                            ["error"] = context.Error,
                            ["error_description"] = context.ErrorDescription,
                            ["error_uri"] = context.ErrorUri
                        };

                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = 401;

                        return context.Response.WriteAsync(payload.ToString());
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            // options.FallbackPolicy = new AuthorizationPolicyBuilder()
            //     .RequireAuthenticatedUser()
            //     .Build();

            options.AddPolicy("RequireAdmin", policy =>
                policy.RequireClaim("role", "Admin"));
            options.AddPolicy("RequireUser", policy =>
                policy.RequireClaim("role", "User"));
            options.AddPolicy("RequireManager", policy =>
                policy.RequireClaim("role", "Manager"));
            options.AddPolicy("RequireUser", policy =>
                policy.RequireClaim("role", "User"));
            options.AddPolicy("RequireDoctor", policy =>
                policy.RequireClaim("role", "Doctor"));
        });

        return services;
    }
}