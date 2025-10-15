using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Notification.API.Extensions;

public static class JwtMinimalValidationExtensions
{
    public static AuthenticationBuilder AddMinimalJwtValidation(this IServiceCollection services, IConfiguration config)
    {
        // Lấy public key XML từ appsettings
        string? xmlKey = config["Jwt:PublicKeyXml"];

        if (string.IsNullOrWhiteSpace(xmlKey))
            throw new InvalidOperationException("Missing Jwt:PublicKeyXml in configuration.");

        // Tạo RSA key từ chuỗi XML
        var rsaKey = RSA.Create();
#pragma warning disable SYSLIB0023
        rsaKey.FromXmlString(xmlKey);
#pragma warning restore SYSLIB0023

        // Cấu hình xác thực JWT
        return services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new RsaSecurityKey(rsaKey)
                };
            });
    }
}