using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using BuildingBlocks.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

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
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var redisConnection = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();

                        var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);

                        if (string.IsNullOrEmpty(jti))
                        {
                            logger.LogWarning("Token validation failed. JTI claim is missing.");
                            context.Fail("Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.");
                            return;
                        }

                        var db = redisConnection.GetDatabase();
                        var redisKey = MyStrings.GenerateRevokedTokenKey(jti);
                        ;

                        try
                        {
                            if (await db.KeyExistsAsync(redisKey))
                            {
                                logger.LogInformation("Token validation failed. Token with JTI {Jti} has been revoked.", jti);
                                context.Fail("This token has been revoked.");
                            }
                        }
                        catch (RedisConnectionException ex)
                        {
                            logger.LogError(ex, "Could not connect to Redis. Deciding on fail-open/fail-closed strategy.");
                            // context.Fail("Authentication service is unavailable.");
                        }
                    }
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