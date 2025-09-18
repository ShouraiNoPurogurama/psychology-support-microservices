using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

    public async Task<(string Token, string Jti)> GenerateJWTToken(User user)
    {
        var roles = await userManager.GetRolesAsync(user);

        var subjectRef = await ResolveSubjectRef(user);

        var onboardingStatus = user.OnboardingStatus.ToString();

        var jti = Guid.NewGuid().ToString();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Sub, subjectRef),
            new Claim(ClaimTypes.Role, string.Join(",", roles)),
            new Claim(ClaimTypes.Authentication, onboardingStatus)
        };

        var rsaSecurityKey = GetRsaSecurityKey();

        var signingCredential = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddHours(1),
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
        var rsaKeys = RSA.Create();
        string xmlKey = File.ReadAllText(configuration.GetSection("Jwt:PrivateKeyPath").Value!);
        rsaKeys.FromXmlString(xmlKey);
        var rsaSecurityKey = new RsaSecurityKey(rsaKeys);
        return rsaSecurityKey;
    }


    public Guid GetAliasIdFromHttpContext(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.ContainsKey("Authorization"))
            throw new ForbiddenException("Thiếu header Authorization.");

        var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            throw new ForbiddenException("Định dạng header Authorization không hợp lệ.");

        var jwtToken = authorizationHeader["Bearer ".Length..]; //Slice token to get Payload

        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(jwtToken)) throw new ForbiddenException("Định dạng JWT token không hợp lệ.");

        try
        {
            var token = tokenHandler.ReadJwtToken(jwtToken);
            var userIdClaim = token.Claims.FirstOrDefault(claim => claim.Type == "aliasId");

            if (userIdClaim is null || string.IsNullOrWhiteSpace(userIdClaim.Value))
                throw new ForbiddenException("Thiếu thông tin Alias Id trong token.");

            return Guid.Parse(userIdClaim.Value);
        }
        catch (Exception e)
        {
            throw new InternalServerException($"Lỗi khi phân tích token: {e.Message}");
        }
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
        var randomNumber = new byte[32]; //Needed for HMAC-SHA512 (256-bit length)
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task SaveRefreshToken(User user, string refreshToken)
    {
        await userManager.SetAuthenticationTokenAsync(user, "PsychologySupport", "RefreshToken", refreshToken);
    }

    public async Task<bool> ValidateRefreshToken(User user, string refreshToken)
    {
        var storedToken = await userManager.GetAuthenticationTokenAsync(user, "PsychologySupport", "RefreshToken");

        return storedToken is not null && storedToken == refreshToken;
    }


    public async Task<(string newJwtToken, string newRefreshToken)> RefreshToken(User user, string refreshToken)
    {
        var isValidRefreshToken = await ValidateRefreshToken(user, refreshToken);

        if (!isValidRefreshToken) throw new ForbiddenException("Refresh Token không hợp lệ.");

        var newJwtToken = await GenerateJWTToken(user);
        var newRefreshToken = GenerateRefreshToken();

        await SaveRefreshToken(user, newRefreshToken);

        return (newJwtToken.Token, newRefreshToken);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var securityKey = GetRsaSecurityKey();
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateActor = false,
            ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            // IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jtw:Key"]!)),
            IssuerSigningKey = securityKey,
            ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        SecurityToken securityToken;

        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

        // var jwtSecurityToken = securityToken as JwtSecurityToken;
        // if (jwtSecurityToken == null ||
        //     !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        //     throw new SecurityTokenException("Token không hợp lệ");

        return principal;
    }
}