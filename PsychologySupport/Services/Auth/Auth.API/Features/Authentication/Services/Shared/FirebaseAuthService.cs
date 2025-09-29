using Auth.API.Features.Authentication.Dtos.Requests;
using Auth.API.Features.Authentication.Dtos.Responses;
using Auth.API.Features.Authentication.ServiceContracts.Shared;
using BuildingBlocks.Constants;
using BuildingBlocks.Exceptions;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace Auth.API.Features.Authentication.Services.Shared
{
    public class FirebaseAuthService(
        UserManager<User> userManager,
        ITokenService tokenService,
        IConfiguration config
    ) : IFirebaseAuthService
    {
        public async Task<LoginResponse> FirebaseLoginAsync(FirebaseLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.FirebaseToken))
                throw new BadRequestException("Firebase token không hợp lệ.");

            var projectId = config["Firebase:ProjectId"];
            string serviceAccountPath = config["Firebase:ServiceAccountPath"];
            
            
            FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(serviceAccountPath),
                    ProjectId = projectId
                });
            
            FirebaseToken decodedToken;
            try
            {
                decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.FirebaseToken);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Firebase token không hợp lệ: ", ex);
            }

            string firebaseUserId = decodedToken.Uid;
            string email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : null;
            string fullName = decodedToken.Claims.ContainsKey("name") ? decodedToken.Claims["name"].ToString() : null;

            var user = await userManager.Users.FirstOrDefaultAsync(u =>
                u.FirebaseUserId == firebaseUserId || u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    FirebaseUserId = firebaseUserId,
                    EmailConfirmed = true,
                    UserName = email
                };

                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new BadRequestException($"Tạo tài khoản người dùng thất bại: {errors}");
                }

                await userManager.AddToRoleAsync(user, Roles.UserRole);
            }

            var (accessToken, jti, refreshToken) = await tokenService.GenerateTokensAsync(user);


            return new LoginResponse(accessToken, refreshToken);
        }
    }
}