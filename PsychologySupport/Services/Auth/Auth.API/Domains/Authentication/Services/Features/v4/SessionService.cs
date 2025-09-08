using System.IdentityModel.Tokens.Jwt;
using Auth.API.Data;
using Auth.API.Domains.Authentication.Dtos.Responses;
using Auth.API.Domains.Authentication.Exceptions;
using Auth.API.Domains.Authentication.ServiceContracts.Features.v4;
using Auth.API.Domains.Authentication.ServiceContracts.Shared;
using BuildingBlocks.Exceptions;

namespace Auth.API.Domains.Authentication.Services.Features.v4;

public class SessionService(
    UserManager<User> userManager,
    AuthDbContext authDbContext,
    ITokenService tokenService,
    IDeviceManagementService deviceManagementService,
    IAuthenticationService authenticationService
    )
    : ISessionService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        //1. Tìm và validate user
        var user = await authenticationService.AuthenticateWithPasswordAsync(loginRequest);

        //2. Xử lý device và session
        var device = await deviceManagementService.GetOrUpsertDeviceAsync(user.Id, loginRequest.ClientDeviceId!,
            loginRequest.DeviceType!.Value,
            loginRequest.DeviceToken);
        await deviceManagementService.ManageDeviceSessionsAsync(user.Id, loginRequest.DeviceType!.Value, device.Id);

        //3. Tạo token
        var (accessToken, refreshToken) = await GenerateTokensAsync(user, device.Id);

        return new LoginResponse(accessToken, refreshToken);
    }

    public async Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request)
    {
        var user = await authenticationService.AuthenticateWithGoogleAsync(request);

        //1. Xử lý device và session
        var device = await deviceManagementService.GetOrUpsertDeviceAsync(user.Id, request.ClientDeviceId!,
            request.DeviceType!.Value,
            request.DeviceToken);
        await deviceManagementService.ManageDeviceSessionsAsync(user.Id, request.DeviceType!.Value, device.Id);

        //2. Tạo token
        var (accessToken, refreshToken) = await GenerateTokensAsync(user, device.Id);

        await authDbContext.SaveChangesAsync();

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

        // Cập nhật lại token
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
}