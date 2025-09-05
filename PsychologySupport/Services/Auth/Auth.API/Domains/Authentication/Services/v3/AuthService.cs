using System.IdentityModel.Tokens.Jwt;
using Auth.API.Data;
using Auth.API.Domains.Authentication.Dtos.Responses;
using Auth.API.Domains.Authentication.Exceptions;
using Auth.API.Domains.Authentication.ServiceContracts;
using Auth.API.Domains.Encryption.Dtos;
using Auth.API.Domains.Encryption.ServiceContracts;
using BuildingBlocks.Constants;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using BuildingBlocks.Utils;
using Google.Apis.Auth;
using Mapster;
using MassTransit;
using Notification.API.Protos;
using Profile.API.Protos;

namespace Auth.API.Domains.Authentication.Services.v3;

public class AuthService(
    UserManager<User> userManager,
    ILogger<AuthService> logger,
    IConfiguration configuration,
    ITokenService tokenService,
    IPayloadProtector payloadProtector,
    NotificationService.NotificationServiceClient notificationClient, // gRPC client
    AuthDbContext authDbContext,
    IPublishEndpoint publishEndpoint,
    IWebHostEnvironment env
) : ServiceContracts.v3.IAuthService
{
    private const int LockoutTimeInMinutes = 15;

    public async Task<bool> RegisterAsync(RegisterRequest registerRequest)
    {
        var existingUser = await userManager.FindByEmailAsync(registerRequest.Email);
        existingUser ??= await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == registerRequest.PhoneNumber);

        if (existingUser is not null) throw new ConflictException("Email hoặc số điện thoại đã tồn tại trong hệ thống");

        var user = registerRequest.Adapt<User>();
        user.Email = user.UserName = registerRequest.Email;

        var result = await userManager.CreateAsync(user, registerRequest.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(ie => ie.Description));
            throw new InvalidDataException($"Đăng ký thất bại: {errors}");
        }

        //Tạo pending verification
        var pendingVerificationUser = new PendingVerificationUser
        {
            UserId = user.Id
        };

        var dto = new PendingSeedDto(registerRequest.FullName, registerRequest.Gender, registerRequest.BirthDate,
            new BuildingBlocks.Data.Common.ContactInfo
            {
                Address = "None",
                Email = registerRequest.Email,
                PhoneNumber = registerRequest.PhoneNumber,
            });

        pendingVerificationUser.PayloadProtected = payloadProtector.Protect(dto);

        authDbContext.PendingVerificationUsers.Add(pendingVerificationUser);

        await authDbContext.SaveChangesAsync();

        await AssignUserRoleAsync(user);
        await SendEmailConfirmationAsync(user);

        return true;
    }

    public async Task<string> ConfirmEmailAsync(ConfirmEmailRequest confirmEmailRequest)
    {
        var token = confirmEmailRequest.Token;
        var email = confirmEmailRequest.Email;
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Email hoặc Token bị thiếu.");

        var user = await userManager.FindByEmailAsync(email)
                   ?? throw new UserNotFoundException(email);

        var result = await userManager.ConfirmEmailAsync(user, token);

        string status = result.Succeeded ? "success" : "failed";
        string message;

        if (result.Succeeded)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);

            await CreateUserProfileAsync(user);

            message = "Xác nhận email thành công.";
        }
        else
        {
            logger.LogError($"*** Xác nhận email thất bại cho user {user.Id}. \n[Details] {string.Join("; ", result.Errors.Select(e => e.Description))}");
            message = $"Xác nhận email thất bại.";
        }

        var baseRedirectUrl = configuration["Mail:ConfirmationRedirectUrl"];
        var redirectUrl = $"{baseRedirectUrl}?status={status}&message={Uri.EscapeDataString(message)}";

        return redirectUrl;
    }

    public async Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request)
    {
        //1. Xác thực Google ID Token
        var payload = await ValidateGoogleTokenAsync(request.GoogleIdToken);

        //2. Tìm hoặc tạo user
        var user = await FindOrCreateGoogleUserAsync(payload);

        //3. Kiểm tra lockout
        ValidateUserLockout(user);

        //4. Xử lý device và session
        var device = await GetOrUpsertDeviceAsync(user.Id, request.ClientDeviceId!, request.DeviceType!.Value,
            request.DeviceToken);
        await ManageDeviceSessionsAsync(user.Id, request.DeviceType!.Value, device.Id);

        //5. Tạo token
        var (accessToken, refreshToken) = await GenerateTokensAsync(user, device.Id);
        
        await authDbContext.SaveChangesAsync();
        
        return new LoginResponse(accessToken, refreshToken);
    }

    public async Task<bool> UnlockAccountAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email)
                   ?? throw new UserNotFoundException(email);

        await userManager.SetLockoutEnabledAsync(user, false);
        await userManager.SetLockoutEndDateAsync(user, null);

        return true;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email)
                   ?? throw new UserNotFoundException(email);

        if (!await userManager.IsEmailConfirmedAsync(user))
            throw new InvalidOperationException("Email chưa được xác nhận.");

        await SendPasswordResetEmailAsync(user);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
                   ?? throw new UserNotFoundException(request.Email);

        if (!await userManager.IsEmailConfirmedAsync(user))
            throw new InvalidOperationException("Email chưa được xác nhận.");

        if (request.NewPassword != request.ConfirmPassword)
            throw new InvalidOperationException("Mật khẩu xác nhận không khớp.");

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Đặt lại mật khẩu thất bại: {errors}");
        }

        return true;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
                   ?? throw new UserNotFoundException(request.Email);

        if (!await userManager.IsEmailConfirmedAsync(user))
            throw new InvalidOperationException("Email chưa được xác nhận.");

        if (request.NewPassword != request.ConfirmPassword)
            throw new InvalidOperationException("Mật khẩu xác nhận không khớp.");

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Đổi mật khẩu thất bại: {errors}");
        }

        return true;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        //1. Tìm và validate user
        var user = await FindAndValidateUserAsync(loginRequest);

        //2. Kiểm tra lockout
        ValidateUserLockout(user);

        //3. Verify password
        await VerifyPasswordAsync(user, loginRequest.Password);

        //4. Xử lý device và session
        var device = await GetOrUpsertDeviceAsync(user.Id, loginRequest.ClientDeviceId!, loginRequest.DeviceType!.Value,
            loginRequest.DeviceToken);
        await ManageDeviceSessionsAsync(user.Id, loginRequest.DeviceType!.Value, device.Id);

        //5. Tạo token
        var (accessToken, refreshToken) = await GenerateTokensAsync(user, device.Id);

        return new LoginResponse(accessToken, refreshToken);
    }

    public async Task<LoginResponse> RefreshAsync(TokenApiRequest request)
    {
        var principal = tokenService.GetPrincipalFromExpiredToken(request.Token)
                        ?? throw new BadRequestException("Access token không hợp lệ");

        var userId = principal.Claims.First(c => c.Type == "userId").Value;
        var jti = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        var user = await userManager.FindByIdAsync(userId)
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

        session.AccessTokenId = newAccessToken.Jti;
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

        var user = await userManager.FindByIdAsync(userId)
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

    #region Private Helper Methods

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

    private async Task<User> FindOrCreateGoogleUserAsync(GoogleJsonWebSignature.Payload payload)
    {
        var user = await userManager.FindByEmailAsync(payload.Email);

        if (user is not null) return user;

        //Tạo user mới
        user = new User
        {
            Email = payload.Email,
            UserName = payload.Email,
            EmailConfirmed = true,
            PhoneNumberConfirmed = false
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            throw new InvalidDataException(string.Join("; ", createResult.Errors.Select(e => e.Description)));

        await AssignUserRoleAsync(user);

        await CreateUserProfileAsync(user);

        return user;
    }

    private async Task AssignUserRoleAsync(User user)
    {
        var roleResult = await userManager.AddToRoleAsync(user, Roles.UserRole);
        if (!roleResult.Succeeded)
            throw new InvalidDataException("Gán vai trò thất bại");
    }

    private async Task CreateUserProfileAsync(User user)
    {
        var pendingUser = await authDbContext.PendingVerificationUsers
                              .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ProcessedAt == null)
                          ?? throw new NotFoundException("Không tìm thấy dữ liệu người đùng để tạo profile",
                              nameof(PendingVerificationUser));

        var pendingSeedDto = payloadProtector.Unprotect<PendingSeedDto>(pendingUser.PayloadProtected);

        var userRegisteredIntegrationEvent = new UserRegisteredIntegrationEvent(
            UserId: user.Id,
            Email: pendingSeedDto.ContactInfo!.Email,
            PhoneNumber: pendingSeedDto.ContactInfo.PhoneNumber,
            Address: pendingSeedDto.ContactInfo.Address,
            FullName: pendingSeedDto.FullName,
            BirthDate: pendingSeedDto.BirthDate,
            Gender: pendingSeedDto.Gender
        );

        await publishEndpoint.Publish(userRegisteredIntegrationEvent);

        pendingUser.ProcessedAt = DateTimeOffset.UtcNow;
        
        await authDbContext.SaveChangesAsync();
    }


    private async Task SendEmailConfirmationAsync(User user)
    {
        if (await HasSentResetEmailRecentlyAsync(user.Email!))
        {
            throw new RateLimitExceededException(
                "Vui lòng đợi ít nhất 1 phút trước khi gửi lại email xác nhận. Nếu chưa nhận được email, hãy kiểm tra hộp thư rác (spam) hoặc đợi thêm một chút.");
        }

        var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var baseUrl = configuration["Mail:ConfirmationUrl"]!;
        var url = string.Format(baseUrl, Uri.EscapeDataString(emailConfirmationToken), Uri.EscapeDataString(user.Email));

        
        var basePath = Path.Combine(env.ContentRootPath, "Domains", "Authentication", "EmailTemplates");
        var confirmTemplatePath = Path.Combine(basePath, configuration["EmailTemplates:ConfirmEmail"]!);
        
        var confirmBody = RenderTemplate(confirmTemplatePath, new Dictionary<string, string>
        {
            ["ConfirmUrl"] = url,
            ["Year"] = DateTime.UtcNow.Year.ToString()
        });

        var sendEmailIntegrationEvent = new SendEmailIntegrationEvent(user.Email, "Xác nhận tài khoản", confirmBody);

        user.PhoneNumberConfirmed = true;
        await publishEndpoint.Publish(sendEmailIntegrationEvent);
    }

    private async Task SendPasswordResetEmailAsync(User user)
    {
        if (await HasSentResetEmailRecentlyAsync(user.Email!))
        {
            throw new RateLimitExceededException(
                "Vui lòng đợi ít nhất 1 phút trước khi gửi lại email đổi mật khẩu. Nếu chưa nhận được email, hãy kiểm tra hộp thư rác (spam) hoặc đợi thêm một chút.");
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrlTemplate = configuration["Mail:PasswordResetUrl"];
        var callbackUrl = string.Format(resetUrlTemplate!, Uri.EscapeDataString(token), Uri.EscapeDataString(user.Email));

        var basePath = Path.Combine(env.ContentRootPath, "Domains", "Authentication", "EmailTemplates");
        var resetTemplatePath = Path.Combine(basePath, configuration["EmailTemplates:ResetPassword"]!);
        
        var resetBody = RenderTemplate(resetTemplatePath, new Dictionary<string, string>
        {
            ["ResetUrl"] = callbackUrl,
            ["Year"] = DateTime.UtcNow.Year.ToString()
        });

        var sendEmailEvent = new SendEmailIntegrationEvent(user.Email, "Khôi phục mật khẩu", resetBody);
        await publishEndpoint.Publish(sendEmailEvent);
    }

    private async Task<bool> HasSentResetEmailRecentlyAsync(string email)
    {
        var grpcRequest = new Notification.API.Protos.HasSentEmailRecentlyRequest { Email = email };
        var response = await notificationClient.HasSentEmailRecentlyAsync(grpcRequest);
        return response.IsRecentlySent;
    }

    private async Task<User> FindAndValidateUserAsync(LoginRequest loginRequest)
    {
        User user;

        if (!string.IsNullOrWhiteSpace(loginRequest.Email))
        {
            user = await userManager.Users
                       .FirstOrDefaultAsync(u => u.Email == loginRequest.Email && !u.LockoutEnabled)
                   ?? throw new UserNotFoundException(loginRequest.Email);

            if (!user.EmailConfirmed)
                throw new ForbiddenException("Tài khoản chưa được xác nhận. Vui lòng kiểm tra email.");
        }
        else
        {
            user = await userManager.Users
                       .FirstOrDefaultAsync(u => u.PhoneNumber == loginRequest.PhoneNumber && !u.LockoutEnabled)
                   ?? throw new UserNotFoundException(loginRequest.PhoneNumber);

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

        // Reset lockout counter
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        await userManager.UpdateAsync(user);
    }

    private async Task<Device> GetOrUpsertDeviceAsync(Guid userId, string clientDeviceId, DeviceType deviceType,
        string? deviceToken)
    {
        var device = await authDbContext.Devices.FirstOrDefaultAsync(d =>
            d.ClientDeviceId == clientDeviceId && d.UserId == userId);

        if (device is null)
        {
            device = new Device
            {
                UserId = userId,
                ClientDeviceId = clientDeviceId,
                DeviceType = deviceType,
                DeviceToken = deviceToken,
                LastUsedAt = DateTime.UtcNow
            };
            authDbContext.Devices.Add(device);
        }
        else
        {
            device.ClientDeviceId = clientDeviceId;
            device.DeviceToken = deviceToken;
            device.LastUsedAt = DateTime.UtcNow;
            device.DeviceType = deviceType;
            authDbContext.Devices.Update(device);
        }

        await authDbContext.SaveChangesAsync();
        return device;
    }

    private async Task ManageDeviceSessionsAsync(Guid userId, DeviceType deviceType, Guid deviceId)
    {
        int allowedLimit = deviceType switch
        {
            DeviceType.Web => 2,
            DeviceType.IOS => 1,
            DeviceType.Android => 1,
            _ => 1
        };

        var allDeviceIds = await authDbContext.Devices
            .Where(d => d.UserId == userId && d.DeviceType == deviceType)
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
    }

    private async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user, Guid deviceId)
    {
        var accessToken = await tokenService.GenerateJWTToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        var session = new DeviceSession
        {
            DeviceId = deviceId,
            AccessTokenId = accessToken.Jti,
            RefreshToken = refreshToken,
            CreatedAt = DateTimeOffset.UtcNow,
            LastRefeshToken = DateTimeOffset.UtcNow
        };

        authDbContext.DeviceSessions.Add(session);

        return (accessToken.Token, refreshToken);
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

    #endregion
}