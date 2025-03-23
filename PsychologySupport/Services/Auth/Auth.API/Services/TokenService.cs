using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Auth.API.Models;
using Auth.API.ServiceContracts;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Profile;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Exception = System.Exception;

namespace Auth.API.Services;

public class TokenService(
    UserManager<User> userManager,
    IConfiguration configuration,
    IRequestClient<GetPatientProfileRequest> patientClient,
    IRequestClient<GetDoctorProfileRequest> doctorClient) : ITokenService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task<string> GenerateJWTToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var profileId = Guid.Empty;

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
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", user.Id.ToString()),
            new Claim("profileId", profileId.ToString()),
            new Claim("role", string.Join(",", roles)),
            new Claim("name", user.UserName!)
        };

        var rsaSecurityKey = GetRsaSecurityKey();

        var signingCredential = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private RsaSecurityKey GetRsaSecurityKey()
    {
        var rsaKeys = RSA.Create();
        string xmlKey = File.ReadAllText(configuration.GetSection("Jwt:PrivateKeyPath").Value!);
        rsaKeys.FromXmlString(xmlKey);
        var rsaSecurityKey = new RsaSecurityKey(rsaKeys);
        return rsaSecurityKey;
    }


    public Guid GetUserIdFromHttpContext(HttpContext httpContext)
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
            var userIdClaim = token.Claims.FirstOrDefault(claim => claim.Type == "userId");

            if (userIdClaim is null || string.IsNullOrWhiteSpace(userIdClaim.Value))
                throw new ForbiddenException("Thiếu thông tin User Id trong token.");

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

        return (newJwtToken, newRefreshToken);
    }

    public ClaimsPrincipal GetPrincipalFromToken(string token)
    {
        var securityKey = GetRsaSecurityKey();
        var validation = new TokenValidationParameters()
        {
            ValidateActor = false,
            ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
        };
        
        return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateActor = false,
            ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jtw:Key"]!)),
            ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;

        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Token không hợp lệ");

        return principal;
    }
}