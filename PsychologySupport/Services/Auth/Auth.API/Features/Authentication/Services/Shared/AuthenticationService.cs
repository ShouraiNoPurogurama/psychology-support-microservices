using Auth.API.Features.Authentication.Dtos.Requests;
using Auth.API.Features.Authentication.Exceptions;
using Auth.API.Features.Authentication.ServiceContracts.Shared;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Utils;
using Google.Apis.Auth;

namespace Auth.API.Features.Authentication.Services.Shared;

public class AuthenticationService(
    UserManager<User> userManager,
    ITokenService tokenService,
    IUserProvisioningService userProvisioningService
)
    : IAuthenticationService
{
    private const int LockoutTimeInMinutes = 15;


    public async Task<User> AuthenticateWithPasswordAsync(LoginRequest request)
    {
        var user = await FindAndValidateUserAsync(request);

        ValidateUserLockout(user);

        await VerifyPasswordAsync(user, request.Password);

        return user;
    }

    public async Task<User> AuthenticateWithGoogleAsync(GoogleLoginRequest request)
    {
        var payload = await ValidateGoogleTokenAsync(request.GoogleIdToken);
        var user = await userProvisioningService.FindOrCreateGoogleUserAsync(payload); // Vẫn cần UserProvisioning
        ValidateUserLockout(user);
        return user;
    }

    private async Task<User> FindAndValidateUserAsync(LoginRequest loginRequest)
    {
        User user;

        if (!string.IsNullOrWhiteSpace(loginRequest.Email))
        {
            user = await userManager.Users
                       .Include(u => u.UserRoles)
                       .ThenInclude(ur => ur.Role)
                       .FirstOrDefaultAsync(u => u.Email == loginRequest.Email && !u.LockoutEnabled)
                   ?? throw new UserNotFoundException("Không tìm thấy tài khoản trong hệ thống. Vui lòng kiểm tra lại email hoặc số điện thoại.");

            if (!user.EmailConfirmed)
                throw new ForbiddenException("Tài khoản chưa được xác nhận. Vui lòng kiểm tra email.");
        }
        else
        {
            user = await userManager.Users
                       .Include(u => u.UserRoles)
                       .ThenInclude(ur => ur.Role)
                       .FirstOrDefaultAsync(u => u.PhoneNumber == loginRequest.PhoneNumber && !u.LockoutEnabled)
                   ?? throw new UserNotFoundException("Không tìm thấy tài khoản trong hệ thống. Vui lòng kiểm tra lại email hoặc số điện thoại.");

            if (user is { PhoneNumberConfirmed: false, PasswordHash: not null })
                throw new ForbiddenException("Tài khoản chưa xác nhận bằng số điện thoại.");
        }

        return user;
    }

    private void ValidateUserLockout(User user)
    {
        var currentTime = CoreUtils.SystemTimeNow;

        if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > currentTime)
        {
            var remain = user.LockoutEnd.Value - currentTime;
            throw new ForbiddenException($"Tài khoản bị khóa. Vui lòng thử lại sau {remain.TotalMinutes:N0} phút.");
        }
    }

    private async Task VerifyPasswordAsync(User user, string password)
    {
        var currentTime = CoreUtils.SystemTimeUtcNow;

        if (!tokenService.VerifyPassword(password, user.PasswordHash!, user))
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 3)
            {
                user.LockoutEnd = currentTime.AddMinutes(LockoutTimeInMinutes);
                await userManager.UpdateAsync(user);
                throw new ForbiddenException("Sai quá số lần quy định. Tài khoản bị khóa tạm thời.");
            }

            await userManager.UpdateAsync(user);
            throw new ForbiddenException("Email hoặc mật khẩu không hợp lệ.");
        }

        //Chỉ cập nhật nếu có sự thay đổi, tránh gọi DB không cần thiết
        if (user.AccessFailedCount > 0 || user.LockoutEnd != null)
        {
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            await userManager.UpdateAsync(user);
        }

        ;
    }

    private async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string googleIdToken)
    {
        try
        {
            return await GoogleJsonWebSignature.ValidateAsync(googleIdToken);
        }
        catch
        {
            throw new BadRequestException("Google token không hợp lệ");
        }
    }
}