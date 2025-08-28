using Auth.API.Models;
using Auth.API.ServiceContracts;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.LifeStyle;
using BuildingBlocks.Messaging.Events.Profile;
using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Exception = System.Exception;

namespace Auth.API.Services;

public class TokenService(
    UserManager<User> userManager,
    IConfiguration configuration,
    IRequestClient<GetPatientProfileRequest> patientClient,
    IRequestClient<GetDoctorProfileRequest> doctorClient,
    IRequestClient<GetUserSubscriptionRequest> subscriptionClient,
    IRequestClient<CheckPatientEmotionTodayRequest> lifestyleClient
    ) : ITokenService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task<(string Token, string Jti)> GenerateJWTToken(User user)
    {
        var roles = await userManager.GetRolesAsync(user);

        var profileId = Guid.Empty;
        var isProfileCompleted = false;
        var hasEmotionLoggedToday = false;

        if (roles.Contains("Doctor"))
        {
            var doctorProfile =
                await doctorClient.GetResponse<GetDoctorProfileResponse>(new GetDoctorProfileRequest(Guid.Empty, user.Id));
            profileId = doctorProfile.Message.Id;
        }
        else if (roles.Contains("User"))
        {
            var patientProfile =
                await patientClient.GetResponse<GetPatientProfileResponse>(new GetPatientProfileRequest(Guid.Empty, user.Id));
            profileId = patientProfile.Message.Id;

            isProfileCompleted = patientProfile.Message.IsProfileCompleted;

            var PatietnEmotion = (await lifestyleClient.GetResponse<CheckPatientEmotionTodayResponse>(
                new CheckPatientEmotionTodayRequest(profileId)));

            hasEmotionLoggedToday = PatietnEmotion.Message.HasLoggedToday;

        }

        var subscriptionResponse = await subscriptionClient.GetResponse<GetUserSubscriptionResponse>(
            new GetUserSubscriptionRequest(profileId));

        var subscriptionPlan = subscriptionResponse.Message.PlanName;

        var jti = Guid.NewGuid().ToString(); // JWT ID (JTI) for unique identification of the token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim("aliasId", user.Id.ToString()),
            new Claim("subscription", subscriptionPlan),
            new Claim("IsProfileCompleted", isProfileCompleted.ToString()),
            new Claim("HasEmotionLoggedToday", hasEmotionLoggedToday.ToString()),
            new Claim(ClaimTypes.Role, string.Join(",", roles))
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

        var jwtToken = authorizationHeader["Bearer ".Length..]; // Slice token to get Payload

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