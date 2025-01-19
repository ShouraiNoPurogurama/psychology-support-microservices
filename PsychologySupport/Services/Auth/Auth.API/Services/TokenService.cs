using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Auth.API.Models;
using Auth.API.ServiceContracts;
using BuildingBlocks.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Exception = System.Exception;

namespace Auth.API.Services;

public class TokenService(UserManager<User> userManager, IConfiguration configuration) : ITokenService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task<string> GenerateJWTToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jtw:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
        var roles = await userManager.GetRolesAsync(user);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email!), //Subject
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //JwtId
            new Claim("userid", user.Id.ToString()),
            new Claim(ClaimTypes.Role, string.Join(",", roles)),
            new Claim("name", user.FullName),
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Guid GetUserIdFromHttpContext(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.ContainsKey("Authorization"))
        {
            throw new ForbiddenException("Authorization header is missing.");
        }

        var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            throw new ForbiddenException("Invalid Authorization header format.");
        }

        var jwtToken = authorizationHeader["Bearer ".Length..]; //Slice the token to get the Payload

        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(jwtToken))
        {
            throw new ForbiddenException("Invalid JWT token format.");
        }

        try
        {
            var token = tokenHandler.ReadJwtToken(jwtToken);
            var userIdClaim = token.Claims.FirstOrDefault(claim => claim.Type == "userid");

            if (userIdClaim is null || string.IsNullOrWhiteSpace(userIdClaim.Value))
            {
                throw new ForbiddenException("User Id claim is missing in the token.");
            }

            return Guid.Parse(userIdClaim.Value);
        }
        catch (Exception e)
        {
            throw new InternalServerException($"Error parsing token: {e.Message}");
        }
    }

    public bool VerifyPassword(string providedPassword, string hashedPassword, User user)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success;
    }

    /// <summary>
    /// Hashes the provided password using the HMACSHA512 algorithm with a salt.
    /// - Salt size: 16 bytes (128 bits).
    /// - Hash size: 32 bytes (256 bits).
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
        var randomNumber = new byte[64]; //Needed for HMAC-SHA512 (512-bit length)
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

        if (!isValidRefreshToken)
        {
            throw new ForbiddenException("Invalid Refresh Token.");
        }

        var newJwtToken = await GenerateJWTToken(user);
        var newRefreshToken = GenerateRefreshToken();

        await SaveRefreshToken(user, newRefreshToken);

        return (newJwtToken: newJwtToken, newRefreshToken: newRefreshToken);
    }
}