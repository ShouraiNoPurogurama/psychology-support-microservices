using Auth.API.Features.Authentication.Dtos.Requests;

namespace Auth.API.Features.Authentication.ServiceContracts.Features;

public interface IPasswordService
{
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
}