using Auth.API.Features.Authentication.Dtos.Requests;

namespace Auth.API.Features.Authentication.ServiceContracts.Shared;

public interface IAuthenticationService
{
    //Nhận vào thông tin đăng nhập, trả về User nếu thành công, hoặc ném ra lỗi.
    Task<User> AuthenticateWithPasswordAsync(LoginRequest request);
    Task<User> AuthenticateWithGoogleAsync(GoogleLoginRequest request);
}