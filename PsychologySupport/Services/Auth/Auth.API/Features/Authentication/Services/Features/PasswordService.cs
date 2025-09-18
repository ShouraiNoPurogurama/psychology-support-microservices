using Auth.API.Features.Authentication.Dtos.Requests;
using Auth.API.Features.Authentication.Exceptions;
using Auth.API.Features.Authentication.ServiceContracts.Features;
using Auth.API.Features.Authentication.ServiceContracts.Shared;

namespace Auth.API.Features.Authentication.Services.Features;

public class PasswordService(UserManager<User> userManager, IEmailService emailService) : IPasswordService
{
    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email)
                   ?? throw new UserNotFoundException(email);

        if (!await userManager.IsEmailConfirmedAsync(user))
            throw new InvalidOperationException("Email chưa được xác nhận.");

        await emailService.SendPasswordResetEmailAsync(user);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
                   ?? throw new UserNotFoundException(request.Email);

        if (!await userManager.IsEmailConfirmedAsync(user))
            throw new InvalidOperationException("Email chưa được xác nhận.");

        if (request.NewPassword != request.ConfirmPassword)
            throw new InvalidOperationException("Mật khẩu xác nhận không khớp.");

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Đặt lại mật khẩu thất bại: {errors}");
        }

        return true;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
                   ?? throw new UserNotFoundException(request.Email);

        if (!await userManager.IsEmailConfirmedAsync(user))
            throw new InvalidOperationException("Email chưa được xác nhận.");

        if (request.NewPassword != request.ConfirmPassword)
            throw new InvalidOperationException("Mật khẩu xác nhận không khớp.");

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Đổi mật khẩu thất bại: {errors}");
        }

        return true;
    }
}