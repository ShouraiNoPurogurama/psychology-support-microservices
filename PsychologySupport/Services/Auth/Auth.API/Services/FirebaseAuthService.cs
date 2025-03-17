using Auth.API.Dtos.Requests;
using Auth.API.Dtos.Responses;
using Auth.API.Models;
using Auth.API.ServiceContracts;
using BuildingBlocks.Constants;
using BuildingBlocks.Exceptions;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Services
{

    public class FirebaseAuthService(
    UserManager<User> _userManager,
    ITokenService _tokenService) : IFirebaseAuthService
    {
        public async Task<LoginResponse> FirebaseLoginAsync(FirebaseLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.FirebaseToken))
                throw new BadRequestException("Firebase token không hợp lệ.");

            // Verify the Firebase ID Token
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.FirebaseToken);
            string uid = decodedToken.Uid;

            // Get user details from Firebase
            var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
            var fullName = userRecord.DisplayName;  
            var email = userRecord.Email;
            //string email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : null;


            // Check user in db
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id.ToString() == uid || u.Email == email);

            if (user == null)
            {
                // Create
                user = new User
                {
                    //Id = uid,
                    FullName = fullName,
                    Email = email,
                    EmailConfirmed = true // auto
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(ie => ie.Description));
                    throw new InvalidDataException($"Tạo tài khoản thất bại: {errors}");
                }

                await _userManager.AddToRoleAsync(user, Roles.UserRole);
            }

            // Create JWT token
            var accessToken = await _tokenService.GenerateJWTToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            await _tokenService.SaveRefreshToken(user, refreshToken);

            return new LoginResponse(accessToken, refreshToken);
        }
    }
}
