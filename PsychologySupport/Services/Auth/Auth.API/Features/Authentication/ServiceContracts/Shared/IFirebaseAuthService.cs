using Auth.API.Features.Authentication.Dtos.Requests;
using Auth.API.Features.Authentication.Dtos.Responses;

namespace Auth.API.Features.Authentication.ServiceContracts.Shared
{
    public interface IFirebaseAuthService
    {
        Task<LoginResponse> FirebaseLoginAsync(FirebaseLoginRequest request);
    }
}
