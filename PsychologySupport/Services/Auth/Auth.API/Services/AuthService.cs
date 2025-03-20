using Auth.API.Dtos.Requests;
using Auth.API.Dtos.Responses;
using Auth.API.Exceptions;
using Auth.API.Models;
using Auth.API.ServiceContracts;
using BuildingBlocks.Constants;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Profile;
using BuildingBlocks.Utils;
using Mapster;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Services;

public class AuthService(
    UserManager<User> _userManager,
    IConfiguration configuration,
    ITokenService tokenService) : IAuthService
{
    private const int LockoutTimeInMinutes = 15;

    public async Task<bool> RegisterAsync(RegisterRequest registerRequest)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerRequest.Email);
        existingUser ??= await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == registerRequest.PhoneNumber);

        if (existingUser is not null) throw new InvalidDataException("Email hoặc số điện thoại đã tồn tại trong hệ thống");

        var user = registerRequest.Adapt<User>();
        user.Email = user.UserName = registerRequest.Email;
        user.EmailConfirmed = true;
        user.PhoneNumberConfirmed = true;

        var result = await _userManager.CreateAsync(user, registerRequest.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(ie => ie.Description));
            throw new InvalidDataException($"Đăng ký thất bại: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, Roles.UserRole);
        if (!roleResult.Succeeded) throw new InvalidDataException("Gán vai trò thất bại");

        // var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        // var confirmationUrlTemplate = configuration["Mail:ConfirmationUrl"];
        // var callbackUrl = string.Format(confirmationUrlTemplate, token, registerRequest.Email);
        // await _sendMail.SendConfirmationEmailAsync(registerRequest.Email, callbackUrl);

        return true;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        User user;
        if (!string.IsNullOrWhiteSpace(loginRequest.Email))
        {
            user = await _userManager.Users
                       .FirstOrDefaultAsync(u => u.Email == loginRequest.Email && !u.LockoutEnabled)
                   ?? throw new UserNotFoundException(loginRequest.Email);

            if (!user.EmailConfirmed)
                throw new ForbiddenException("Tài khoản chưa được xác nhận. Vui lòng kiểm tra email để xác nhận tài khoản.");
        }
        else
        {
            user = await _userManager.Users.FirstOrDefaultAsync(u =>
                       u.PhoneNumber == loginRequest.PhoneNumber && !u.LockoutEnabled)
                   ?? throw new UserNotFoundException(loginRequest.PhoneNumber);

            if (!user.PhoneNumberConfirmed)
                throw new ForbiddenException(
                    "Tài khoản chưa được xác nhận. Vui lòng kiểm tra tin nhắn SMS để xác nhận tài khoản.");
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
            token,
            refreshToken
        );
    }


    public async Task<bool> ConfirmEmailAsync(string token, string email)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Email hoặc Token bị thiếu.");

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
        var resetUrlTemplate = configuration["Mail:PasswordResetUrl"]!;
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
    public async Task<LoginResponse> RefreshAsync(TokenApiRequest tokenApiRequest)
    {
        var accessToken = tokenApiRequest.Token;
        var refreshToken = tokenApiRequest.RefreshToken;

        var principal = tokenService.GetPrincipalFromExpiredToken(accessToken)
                        ?? throw new BadRequestException("Access token không hợp lệ");

        var userId = principal.Claims.First(c => c.Type == "userId").Value;

        var user = await _userManager.FindByIdAsync(userId)
                   ?? throw new UserNotFoundException(userId);

        if (!await tokenService.ValidateRefreshToken(user, refreshToken))
            throw new BadRequestException("Refresh token không hợp lệ");

        var newAccessToken = await tokenService.GenerateJWTToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        return new LoginResponse(newAccessToken, newRefreshToken);
    }
}