namespace Auth.API.Domains.Authentication.ServiceContracts.Features.v4;

public interface IPasswordService
{
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
}