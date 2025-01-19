using Auth.API.Dtos.Requests;
using Auth.API.Dtos.Responses;
using Auth.API.Models;
using Auth.API.ServiceContracts;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.Services;

public class AuthService(UserManager<User> _userManager, IConfiguration config, ITokenService tokenService) : IAuthService
{
    public Task<bool> RegisterUser(RegisterRequest registerRequest)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ConfirmEmailAsync(string token, string email)
    {
        throw new NotImplementedException();
    }

    public Task<LoginResponse> Login(LoginRequest loginRequest)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UnlockAccountAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ForgotPasswordAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<LoginResponse> RefreshToken(string refreshToken)
    {
        throw new NotImplementedException();
    }
}