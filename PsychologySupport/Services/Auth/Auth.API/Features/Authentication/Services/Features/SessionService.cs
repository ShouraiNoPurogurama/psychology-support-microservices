using System.IdentityModel.Tokens.Jwt;
using Auth.API.Data;
using Auth.API.Features.Authentication.Dtos.Requests;
using Auth.API.Features.Authentication.Dtos.Responses;
using Auth.API.Features.Authentication.Exceptions;
using Auth.API.Features.Authentication.ServiceContracts.Features;
using Auth.API.Features.Authentication.ServiceContracts.Shared;
using BuildingBlocks.Exceptions;

namespace Auth.API.Features.Authentication.Services.Features;

public class SessionService(
    UserManager<User> userManager,
    AuthDbContext authDbContext,
    ITokenService tokenService,
    ITokenRevocationService tokenRevocationService,
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
        var device = await deviceManagementService.GetOrUpsertDeviceAsync(user.Id, 
            loginRequest.ClientDeviceId!,
            loginRequest.DeviceType!.Value,
            loginRequest.DeviceToken);
        
        //3. Tạo token mới
        var (accessToken, jti, refreshToken) = await GenerateTokensAsync(user, device.Id);
        await deviceManagementService.CreateDeviceSessionAsync(device.Id, jti, refreshToken);

        //4. KIỂM TRA và thu hồi session CŨ NHẤT nếu vượt giới hạn
        await deviceManagementService.RevokeOldestSessionIfLimitExceededAsync(user.Id, loginRequest.DeviceType.Value, device.Id, jti);

        //5. Trả về token
        return new LoginResponse(accessToken, refreshToken);
    }

    public async Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request)
    {
        var user = await authenticationService.AuthenticateWithGoogleAsync(request);

        //1. Xử lý device và session
        var device = await deviceManagementService.GetOrUpsertDeviceAsync(user.Id, request.ClientDeviceId!,
            request.DeviceType!.Value,
            request.DeviceToken);
        
        //2. Tạo token
        var (accessToken, jti, refreshToken) = await GenerateTokensAsync(user, device.Id);
        await deviceManagementService.CreateDeviceSessionAsync(device.Id, jti, refreshToken);
        
        await deviceManagementService.RevokeOldestSessionIfLimitExceededAsync(user.Id, request.DeviceType!.Value, device.Id, jti);
        
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

        // var userId = principal.Claims.First(c => c.Type == "userId").Value;
        var userId = "6e1a065e-9b00-442b-9be1-d4447e19ff78";
        var jti = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        var user = await userManager.FindByIdAsync(userId)
                   ?? throw new UserNotFoundException(userId);

        var device = await authDbContext.Devices
                         .FirstOrDefaultAsync(d => d.ClientDeviceId == request.ClientDeviceId && d.UserId == user.Id)
                     ?? throw new NotFoundException("Thiết bị không hợp lệ");

        var sessions = await authDbContext.DeviceSessions
            .Where(s =>
                s.DeviceId == device.Id &&
                s.AccessTokenId == jti &&
                s.RefreshToken == request.RefreshToken)
            .ToListAsync();

        if (sessions.Count == 0)
            throw new NotFoundException("Session không tồn tại hoặc đã bị thu hồi");

        await tokenRevocationService.RevokeSessionsAsync(sessions);

        return true;
    }


    private async Task<(string AccessToken, string Jti, string RefreshToken)> GenerateTokensAsync(User user, Guid deviceId)
    {
        var accessToken = await tokenService.GenerateJWTToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        return (accessToken.Token, accessToken.Jti, refreshToken);
    }
}