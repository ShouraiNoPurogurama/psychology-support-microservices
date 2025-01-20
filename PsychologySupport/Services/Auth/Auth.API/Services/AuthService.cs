using Auth.API.Dtos.Requests;
using Auth.API.Dtos.Responses;
using Auth.API.Exceptions;
using Auth.API.Models;
using Auth.API.ServiceContracts;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Services;

public class AuthService(
    UserManager<User> _userManager,
    IConfiguration config,
    ITokenService tokenService) : IAuthService
{
    private const int LockoutTimeInMinutes = 15;

    public Task<bool> Register(RegisterRequest registerRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<LoginResponse> Login(LoginRequest loginRequest)
    {
        var user = await _userManager.Users
                       .FirstOrDefaultAsync(u => u.Email == loginRequest.Email && !u.LockoutEnabled)
                   ?? throw new UserNotFoundException(loginRequest.Email);

        if (!user.EmailConfirmed)
        {
            throw new ForbiddenException("Tài khoản chưa được xác nhận. Vui lòng kiểm tra email để xác nhận tài khoản.");
        }

        var currentTime = CoreUtils.SystemTimeNow;

        if (user is { LockoutEnabled: true, LockoutEnd: not null } && user.LockoutEnd.Value > currentTime)
        {
            var remainingLockoutTime = user.LockoutEnd.Value - currentTime;
            throw new ForbiddenException(
                $"Tài khoản của bạn đã bị khóa. Vui lòng thử lại sau {remainingLockoutTime.TotalMinutes:N0} phút.");
        }

        if (!tokenService.VerifyPassword(loginRequest.Password, user.PasswordHash!, user))
        {
            user.AccessFailedCount++;

            if (user.AccessFailedCount >= 3)
            {
                user.LockoutEnd = currentTime.AddMinutes(LockoutTimeInMinutes);
                await _userManager.UpdateAsync(user);
                throw new ForbiddenException(
                    $"Bạn đã đăng nhập sai quá số lần quy định. Tài khoản đã bị khóa trong {LockoutTimeInMinutes} phút");
            }

            await _userManager.UpdateAsync(user);
            throw new ForbiddenException("Email hoặc mật khẩu không hợp lệ.");
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        await _userManager.UpdateAsync(user);

        var token = await tokenService.GenerateJWTToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        await tokenService.SaveRefreshToken(user, refreshToken);

        return new LoginResponse(
            Token: token,
            RefreshToken: refreshToken
        );
    }


    public async Task<bool> ConfirmEmailAsync(string token, string email)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException("Email hoặc Token bị thiếu.");
        }

        var user = await _userManager.FindByEmailAsync(email)
                   ?? throw new UserNotFoundException(email);

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<bool> UnlockAccountAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email)
                   ?? throw new UserNotFoundException(email);

        await _userManager.SetLockoutEnabledAsync(user, false);
        await _userManager.SetLockoutEndDateAsync(user, null);

        return true;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email)
                   ?? throw new UserNotFoundException(email);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrlTemplate = config["Mail:PasswordResetUrl"]!;
        var callbackUrl = string.Format(resetUrlTemplate, token, email);

        // await _sendMail.SendForgotPasswordEmailAsync(email, callbackUrl);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
                   ?? throw new UserNotFoundException(request.Email);
        
        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return result.Succeeded;
    }

    //TODO Double check this in production env
    public async Task<LoginResponse> Refresh(TokenApiRequest request)
    {
        // var accessToken = request.Token;
        // var refreshToken = request.RefreshToken;
        //
        // var principal = 
        throw new NotImplementedException();
    }
}