using Auth.API.Data;
using Auth.API.Dtos.Requests;
using Auth.API.Dtos.Responses;
using Auth.API.Exceptions;
using Auth.API.Models;
using Auth.API.ServiceContracts;
using BuildingBlocks.Constants;
using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Notification;
using BuildingBlocks.Messaging.Events.Profile;
using BuildingBlocks.Utils;
using Mapster;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.IO;

namespace Auth.API.Services;

public class AuthService(
    UserManager<User> _userManager,
    IConfiguration configuration,
    ITokenService tokenService,
    IRequestClient<CreatePatientProfileRequest> _profileClient,
    AuthDbContext authDbContext,
    IPublishEndpoint publishEndpoint,
    IWebHostEnvironment env
    ) : IAuthService
{
    private const int LockoutTimeInMinutes = 15;

    public async Task<bool> RegisterAsync(RegisterRequest registerRequest)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerRequest.Email);
        existingUser ??= await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == registerRequest.PhoneNumber);

        if (existingUser is not null) throw new InvalidDataException("Email hoặc số điện thoại đã tồn tại trong hệ thống");

        var user = registerRequest.Adapt<User>();
        user.Email = user.UserName = registerRequest.Email;
        
        var result = await _userManager.CreateAsync(user, registerRequest.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(ie => ie.Description));
            throw new InvalidDataException($"Đăng ký thất bại: {errors}");
        }
        
        var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        
        var baseUrl = configuration["Mail:ConfirmationUrl"]!;

        var url = string.Format(
            baseUrl,
            Uri.EscapeDataString(emailConfirmationToken),
            Uri.EscapeDataString(user.Email)
        );

        var confirmTemplatePath = Path.Combine(env.ContentRootPath, "EmailTemplates", "AccountConfirmation.html");

        var confirmBody = RenderTemplate(confirmTemplatePath, new Dictionary<string, string>
        {
            ["ConfirmUrl"] = url,
            ["Year"] = DateTime.UtcNow.Year.ToString()
        });
        var sendEmailIntegrationEvent = new SendEmailIntegrationEvent(
            user.Email,
            "Xác nhận tài khoản",
            confirmBody
        );

        // user.EmailConfirmed = true;
        user.PhoneNumberConfirmed = true;

        var roleResult = await _userManager.AddToRoleAsync(user, Roles.UserRole);
        if (!roleResult.Succeeded) throw new InvalidDataException("Gán vai trò thất bại");
        
        
        await publishEndpoint.Publish(sendEmailIntegrationEvent);
        
        return true;
    }
    public async Task<string> ConfirmEmailAsync(ConfirmEmailRequest confirmEmailRequest)
    {
        var token = confirmEmailRequest.Token; var email = confirmEmailRequest.Email;
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Email hoặc Token bị thiếu.");

        var user = await _userManager.FindByEmailAsync(email)
                   ?? throw new UserNotFoundException(email);

        var result = await _userManager.ConfirmEmailAsync(user, token);

        string status = result.Succeeded ? "success" : "failed";
        string message;

        if (result.Succeeded)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            var contactInfo = ContactInfo.Of("None", user.Email, user.PhoneNumber);
            var createProfileRequest = new CreatePatientProfileRequest(
                user.Id,
                user.FullName,
                user.Gender,
                null,
                PersonalityTrait.None,
                contactInfo
            );

            var profileResponse = await _profileClient.GetResponse<CreatePatientProfileResponse>(createProfileRequest);

            if (!profileResponse.Message.Success)
            {
                status = "partial";
                message = $"Xác nhận email thành công nhưng tạo hồ sơ thất bại: {profileResponse.Message.Message}";
            }
            else
            {
                message = "Xác nhận email và tạo hồ sơ thành công.";
            }
        }
        else
        {
            message = $"Xác nhận email thất bại: {string.Join("; ", result.Errors.Select(e => e.Description))}";
        }

        var baseRedirectUrl = configuration["Mail:ConfirmationRedirectUrl"];
        var redirectUrl = $"{baseRedirectUrl}?status={status}&message={Uri.EscapeDataString(message)}";

        return redirectUrl;
    }

    public Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UnlockAccountAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email)
                   ?? throw new UserNotFoundException(email);

        await _userManager.SetLockoutEnabledAsync(user, false);
        await _userManager.SetLockoutEndDateAsync(user, null);

        return true;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {

        var user = await _userManager.FindByEmailAsync(email)
                   ?? throw new UserNotFoundException(email);

        if (!await _userManager.IsEmailConfirmedAsync(user))
            throw new InvalidOperationException("Email chưa được xác nhận.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var resetUrlTemplate = configuration["Mail:PasswordResetUrl"]; 
        var callbackUrl = string.Format(
            resetUrlTemplate!,
            Uri.EscapeDataString(token),
            Uri.EscapeDataString(email)
        );

        var resetTemplatePath = Path.Combine(env.ContentRootPath,"EmailTemplates", "PasswordReset.html");
        var resetBody = RenderTemplate(resetTemplatePath, new Dictionary<string, string>
        {
            ["ResetUrl"] = callbackUrl,
            ["Year"] = DateTime.UtcNow.Year.ToString()
        });
        var sendEmailEvent = new SendEmailIntegrationEvent(
            user.Email,
            "Khôi phục mật khẩu",
            resetBody
        );

        await publishEndpoint.Publish(sendEmailEvent);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
                   ?? throw new UserNotFoundException(request.Email);

        if (!await _userManager.IsEmailConfirmedAsync(user))
            throw new InvalidOperationException("Email chưa được xác nhận.");

        if (request.NewPassword != request.ConfirmPassword)
            throw new InvalidOperationException("Mật khẩu xác nhận không khớp.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Đặt lại mật khẩu thất bại: {errors}");
        }

        return true;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        User user;
        if (!string.IsNullOrWhiteSpace(loginRequest.Email))
        {
            user = await _userManager.Users
                       .FirstOrDefaultAsync(u => u.Email == loginRequest.Email && !u.LockoutEnabled)
                   ?? throw new UserNotFoundException(loginRequest.Email);

            if (!user.EmailConfirmed)
                throw new ForbiddenException("Tài khoản chưa được xác nhận. Vui lòng kiểm tra email.");
        }
        else
        {
            user = await _userManager.Users
                       .FirstOrDefaultAsync(u => u.PhoneNumber == loginRequest.PhoneNumber && !u.LockoutEnabled)
                   ?? throw new UserNotFoundException(loginRequest.PhoneNumber);

            if (!user.PhoneNumberConfirmed)
                throw new ForbiddenException("Tài khoản chưa xác nhận bằng số điện thoại.");
        }

        var currentTime = CoreUtils.SystemTimeUtcNow;

        if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > currentTime)
        {
            var remain = user.LockoutEnd.Value - currentTime;
            throw new ForbiddenException($"Tài khoản bị khóa. Vui lòng thử lại sau {remain.TotalMinutes:N0} phút.");
        }

        if (!tokenService.VerifyPassword(loginRequest.Password, user.PasswordHash!, user))
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 3)
            {
                user.LockoutEnd = currentTime.AddMinutes(LockoutTimeInMinutes);
                await _userManager.UpdateAsync(user);
                throw new ForbiddenException("Sai quá số lần quy định. Tài khoản bị khóa tạm thời.");
            }

            await _userManager.UpdateAsync(user);
            throw new ForbiddenException("Email hoặc mật khẩu không hợp lệ.");
        }

        // Reset lockout counter
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        await _userManager.UpdateAsync(user);

        // Create or update Device
        var device = await authDbContext.Devices.FirstOrDefaultAsync(d =>
            d.ClientDeviceId == loginRequest.ClientDeviceId && d.UserId == user.Id);

        if (device is null)
        {
            device = new Device
            {
                UserId = user.Id,
                ClientDeviceId = loginRequest.ClientDeviceId!,
                DeviceType = loginRequest.DeviceType!.Value,
                DeviceToken = loginRequest.DeviceToken,
                LastUsedAt = DateTime.UtcNow
            };
            authDbContext.Devices.Add(device);
        }
        else
        {
            device.ClientDeviceId = loginRequest.ClientDeviceId!;
            device.DeviceToken = loginRequest.DeviceToken;
            device.LastUsedAt = DateTime.UtcNow;
            device.DeviceType = loginRequest.DeviceType!.Value;
            authDbContext.Devices.Update(device);
        }

        await authDbContext.SaveChangesAsync();

        // Limit sessions per device type
        int allowedLimit = loginRequest.DeviceType switch
        {
            DeviceType.Web => 2,
            DeviceType.IOS => 1,
            DeviceType.Android => 1,
            _ => 1
        };

        var allDeviceIds = await authDbContext.Devices
            .Where(d => d.UserId == user.Id && d.DeviceType == loginRequest.DeviceType)
            .Select(d => d.Id)
            .ToListAsync();

        var activeSessions = await authDbContext.DeviceSessions
            .Where(s => allDeviceIds.Contains(s.DeviceId) && !s.IsRevoked)
            .OrderBy(s => s.LastRefeshToken ?? s.CreatedAt)
            .ToListAsync();

        if (activeSessions.Count >= allowedLimit)
        {
            var oldestSession = activeSessions.First();
            oldestSession.IsRevoked = true;
            oldestSession.RevokedAt = DateTimeOffset.UtcNow;
        }

        // create access token and refresh token
        var accessToken = await tokenService.GenerateJWTToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        var session = new DeviceSession
        {
            DeviceId = device.Id,
            AccessTokenId = accessToken.Jti,
            RefreshToken = refreshToken,
            CreatedAt = DateTimeOffset.UtcNow,
            LastRefeshToken = DateTimeOffset.UtcNow
        };

        authDbContext.DeviceSessions.Add(session);
        await authDbContext.SaveChangesAsync();

        return new LoginResponse(accessToken.Token, refreshToken);
    }

    public async Task<LoginResponse> RefreshAsync(TokenApiRequest request)
    {
        var principal = tokenService.GetPrincipalFromExpiredToken(request.Token)
                        ?? throw new BadRequestException("Access token không hợp lệ");

        var userId = principal.Claims.First(c => c.Type == "userId").Value;
        var jti = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        var user = await _userManager.FindByIdAsync(userId)
                   ?? throw new UserNotFoundException(userId);

        var device = await authDbContext.Devices
            .FirstOrDefaultAsync(d => d.ClientDeviceId == request.ClientDeviceId && d.UserId == user.Id)
            ?? throw new NotFoundException("Thiết bị không hợp lệ");

        var session = await authDbContext.DeviceSessions
            .FirstOrDefaultAsync(s =>
                s.DeviceId == device.Id &&
                s.AccessTokenId == jti &&
                s.RefreshToken == request.RefreshToken &&
                !s.IsRevoked);

        if (session == null || session.AccessTokenId != jti)
            throw new BadRequestException("Session hoặc Refresh token không hợp lệ");

        /// Cập nhật lại token
        var newAccessToken = await tokenService.GenerateJWTToken(user);
        //var newRefreshToken = tokenService.GenerateRefreshToken();

        session.AccessTokenId = newAccessToken.Jti;
        //session.RefreshToken = newRefreshToken;
        session.LastRefeshToken = DateTimeOffset.UtcNow;

        await authDbContext.SaveChangesAsync();

        return new LoginResponse(newAccessToken.Token, session.RefreshToken);
    }

    public async Task<bool> RevokeAsync(TokenApiRequest request)
    {
        var principal = tokenService.GetPrincipalFromExpiredToken(request.Token)
                        ?? throw new BadRequestException("Access token không hợp lệ");

        var userId = principal.Claims.First(c => c.Type == "userId").Value;
        var jti = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        var user = await _userManager.FindByIdAsync(userId)
                   ?? throw new UserNotFoundException(userId);

        var device = await authDbContext.Devices
            .FirstOrDefaultAsync(d => d.ClientDeviceId == request.ClientDeviceId && d.UserId == user.Id)
            ?? throw new NotFoundException("Thiết bị không hợp lệ");

        var session = await authDbContext.DeviceSessions
            .FirstOrDefaultAsync(s =>
                s.DeviceId == device.Id &&
                s.AccessTokenId == jti &&
                s.RefreshToken == request.RefreshToken);

        if (session == null)
            throw new NotFoundException("Session không tồn tại hoặc đã bị thu hồi");

        session.IsRevoked = true;
        session.RevokedAt = DateTimeOffset.UtcNow;


        await authDbContext.SaveChangesAsync();
        return true;
    }

    private string RenderTemplate(string templatePath, Dictionary<string, string> values)
    {
        var template = File.ReadAllText(templatePath);
        foreach (var pair in values)
        {
            template = template.Replace($"{{{{{pair.Key}}}}}", pair.Value);
        }
        return template;
    }

}