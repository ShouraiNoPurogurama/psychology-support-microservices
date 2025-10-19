using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Auth.API.Enums;
using Auth.API.Features.Authentication.ServiceContracts.Shared;
using BuildingBlocks.Exceptions;
using Microsoft.IdentityModel.Tokens;
using Pii.API.Protos;
using Exception = System.Exception;

namespace Auth.API.Features.Authentication.Services.Shared;

public class TokenService(
    UserManager<User> userManager,
    IConfiguration configuration,
    ILogger<TokenService> logger,
    PiiService.PiiServiceClient piiClient
) : ITokenService
{
    private readonly PasswordHasher<User> _passwordHasher = new();
    private static RsaSecurityKey? _cachedKey;

    public (string Token, string Jti) GenerateJWTFromOldToken(string oldToken)
    {
        var principal = GetPrincipalFromExpiredToken(oldToken);

        var sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(sub, out var _) || string.IsNullOrWhiteSpace(sub))
            throw new UnauthorizedException();

        var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value);
        var onboarding = principal.FindFirst(ClaimTypes.Authentication)?.Value ?? nameof(UserOnboardingStatus.Pending);
        var subscriptionPlanName = principal.FindFirst("SubscriptionPlanName")?.Value;

        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(ClaimTypes.NameIdentifier, sub),
            new Claim(ClaimTypes.Authentication, onboarding)
        };

        if (!string.IsNullOrEmpty(subscriptionPlanName))
        {
            claims.Add(new Claim("SubscriptionPlanName", subscriptionPlanName));
        }

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var rsaSecurityKey = GetRsaSecurityKey();

        var signingCredential = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: DateTimeOffset.UtcNow.UtcDateTime.AddHours(1),
            signingCredentials: signingCredential
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, jti);
    }

    public async Task<(string Token, string Jti)> GenerateJWTToken(User user)
    {
        var roles = user.UserRoles.Select(r => r.Role.Name).ToList();

        var subjectRef = await ResolveSubjectRef(user);

        var onboardingStatus = user.OnboardingStatus.ToString();

        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Sub, subjectRef),
            new Claim(ClaimTypes.Authentication, onboardingStatus),
            new Claim("SubscriptionPlanName", user.SubscriptionPlanName ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role!));
        }

        var rsaSecurityKey = GetRsaSecurityKey();

        var signingCredential = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: DateTimeOffset.Now.AddHours(1).DateTime,
            signingCredentials: signingCredential
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, jti);
    }


    private async Task<string> ResolveSubjectRef(User user)
    {
        //Cấu hình retry: Thử tối đa 5 lần, mỗi lần cách nhau 300ms.
        //Tổng cộng chờ tối đa ~1.5 giây. 
        //Nếu sau 1.5s mà MQ vẫn chưa xử lý xong thì hệ thống đang có vấn đề nghiêm trọng.
        int maxRetries = 5;
        TimeSpan delayBetweenRetries = TimeSpan.FromMilliseconds(300);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var resolveSubjectRef = await piiClient.ResolveSubjectRefByUserIdAsync(new ResolveSubjectRefByUserIdRequest()
                {
                    UserId = user.Id.ToString()
                });

                var subjectRef = resolveSubjectRef.SubjectRef;

                if (!string.IsNullOrEmpty(subjectRef) && subjectRef != Guid.Empty.ToString())
                {
                    return subjectRef;
                }

                logger.LogWarning("Attempt {Attempt}: Profile not found for User {UserId}. Retrying...", attempt, user.Id);
            }
            catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                //Nếu gRPC service của bạn CHỦ ĐỘNG ném lỗi NotFound thay vì trả về rỗng, 
                //chúng ta cũng bắt lỗi này và coi như "chưa sẵn sàng" để retry.
                //logger.LogWarning(ex, "Attempt {Attempt}: gRPC NotFound for User {UserId}. Retrying...", attempt, user.Id);
            }

            // Nếu chưa phải lần thử cuối, đợi trước khi thử lại
            if (attempt < maxRetries)
            {
                await Task.Delay(delayBetweenRetries);
            }
        }

        //Nếu đã chạy hết vòng lặp (hết 5 lần) mà vẫn KHÔNG TÌM THẤY,
        //lúc này phải ném lỗi thật sự.
        //Việc đăng nhập không thể tiếp tục nếu không có SubjectRef.
        throw new NotReadyException(
            $"Không thể khởi tạo hồ sơ cho người dùng sau {maxRetries} lần thử. Vui lòng thử lại sau.", "PROFILE_NOT_READY");
    }

    private RsaSecurityKey GetRsaSecurityKey()
    {
        if (_cachedKey != null) return _cachedKey;
        var rsa = RSA.Create();
        rsa.FromXmlString(File.ReadAllText(configuration["Jwt:PrivateKeyPath"]!));
        
        //Cache key để tránh việc đọc file và parse XML nhiều lần.
        _cachedKey = new RsaSecurityKey(rsa);
        return _cachedKey;
    }


    public bool VerifyPassword(string providedPassword, string hashedPassword, User user)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        
        return result == PasswordVerificationResult.Success;
    }

    /// <summary>
    ///     Hashes the provided password using the HMACSHA256 algorithm with a salt.
    ///     - Salt size: 16 bytes (128 bits).
    ///     - Hash size: 32 bytes (256 bits).
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password">The plaintext password to be hashed.</param>
    /// <returns>The hashed password as a string.</returns>
    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32]; // (256-bit length)
        RandomNumberGenerator.Fill(randomNumber);
        return Base64UrlEncoder.Encode(randomNumber);
    }

    public (string newJwtToken, string jti, string newRefreshToken) RefreshToken(string oldJwt)
    {
        (string token, string jti) =  GenerateJWTFromOldToken(oldJwt);

        var newRefreshToken = GenerateRefreshToken();

        return (token, jti, newRefreshToken);
    }

    public async Task<(string AccessToken, string Jti, string RefreshToken)> GenerateTokensAsync(User user)
    {
        var accessToken = await GenerateJWTToken(user);

        var refreshToken = GenerateRefreshToken();

        return (accessToken.Token, accessToken.Jti, refreshToken);
    }


    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var securityKey = GetRsaSecurityKey();

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateActor = false,
            
            ValidateAudience = true,
            ValidAudience = configuration["Jwt:Audience"], 
            
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero,
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwt ||
            !string.Equals(jwt.Header.Alg, SecurityAlgorithms.RsaSha256, StringComparison.Ordinal))
        {
            throw new SecurityTokenException("Thuật toán ký không hợp lệ");
        }

        return principal;
    }
}