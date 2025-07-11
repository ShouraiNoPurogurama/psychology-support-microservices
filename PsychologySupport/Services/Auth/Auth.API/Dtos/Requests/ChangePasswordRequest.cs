namespace Auth.API.Dtos.Requests
{
    public record ChangePasswordRequest(
         string Email,
         string CurrentPassword,
         string NewPassword,
         string ConfirmPassword
    );

}
