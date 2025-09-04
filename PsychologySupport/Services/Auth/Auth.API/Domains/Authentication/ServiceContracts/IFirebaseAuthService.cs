using Auth.API.Domains.Authentication.Dtos.Responses;

namespace Auth.API.Domains.Authentication.ServiceContracts
{
    public interface IFirebaseAuthService
    {
        Task<LoginResponse> FirebaseLoginAsync(FirebaseLoginRequest request);
    }
}
