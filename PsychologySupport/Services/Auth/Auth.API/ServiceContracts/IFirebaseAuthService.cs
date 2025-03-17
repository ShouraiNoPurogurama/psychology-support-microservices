using Auth.API.Dtos.Requests;
using Auth.API.Dtos.Responses;

namespace Auth.API.ServiceContracts
{
    public interface IFirebaseAuthService
    {
        Task<LoginResponse> FirebaseLoginAsync(FirebaseLoginRequest request);
    }
}
