namespace Auth.API.Domains.Authentication.Dtos.Requests
{
    public record ChangePasswordRequest(
         string Email,
         string CurrentPassword,
         string NewPassword,
         string ConfirmPassword
    );

}
