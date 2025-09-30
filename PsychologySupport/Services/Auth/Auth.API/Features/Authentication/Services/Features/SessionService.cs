using System.IdentityModel.Tokens.Jwt;
using Auth.API.Common.Authentication;
using Auth.API.Data;
using Auth.API.Features.Authentication.Dtos.Requests;
using Auth.API.Features.Authentication.Dtos.Responses;
using Auth.API.Features.Authentication.ServiceContracts.Features;
using Auth.API.Features.Authentication.ServiceContracts.Shared;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;
using MassTransit;
using Pii.API.Protos;

namespace Auth.API.Features.Authentication.Services.Features;

public class SessionService(
    AuthDbContext dbContext,
    ICurrentActorAccessor actorAccessor,
    PiiService.PiiServiceClient piiClient,
    UserManager<User> userManager,
    AuthDbContext authDbContext,
    ITokenService tokenService,
    ITokenRevocationService tokenRevocationService,
    IDeviceManagementService deviceManagementService,
    IAuthenticationService authenticationService,
    IPublishEndpoint publishEndpoint
)
    : ISessionService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        List<TokenRevokedIntegrationEvent> eventsToPublish = [];

        // Bắt đầu một transaction bao trùm toàn bộ flow
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var user = await authenticationService.AuthenticateWithPasswordAsync(request);

            // 2. Xử lý device và session
            var device = await deviceManagementService.GetOrUpsertDeviceAsync(user.Id,
                request.ClientDeviceId!,
                request.DeviceType!.Value,
                request.DeviceToken);

            // 3. Tạo token mới
            var (accessToken, jti, refreshToken) = await tokenService.GenerateTokensAsync(user);

            deviceManagementService.PrepareNewDeviceSession(device.Id, jti, refreshToken);

            var sessionsToRevoke =
                await deviceManagementService.GetOldestSessionsToRevokeAsync(user.Id, request.DeviceType.Value, device.Id, jti);
            if (sessionsToRevoke.Any())
            {
                deviceManagementService.MarkSessionsAsRevoked(sessionsToRevoke);

                // Chuẩn bị event để publish sau
                eventsToPublish.AddRange(sessionsToRevoke.Select(s =>
                    new TokenRevokedIntegrationEvent(s.AccessTokenId, s.RevokedAt!.Value)));
            }

            await dbContext.SaveChangesAsync();

            // 5. Nếu tất cả các bước trên thành công, commit transaction
            await transaction.CommitAsync();

            // 7. Publish event ra bên ngoài SAU KHI DB đã được ghi thành công
            if (eventsToPublish.Any())
            {
                // Giờ đây event chỉ được publish khi dữ liệu đã chắc chắn nằm trong DB
                await publishEndpoint.PublishBatch(eventsToPublish);
            }

            // 6. Trả về token
            return new LoginResponse(accessToken, refreshToken);
        }
        catch (Exception)
        {
            // 7. Nếu có bất kỳ lỗi nào xảy ra, rollback toàn bộ các thay đổi trong DB
            await transaction.RollbackAsync();

            // Ném lại exception để các middleware phía trên xử lý
            throw;
        }
    }

    public async Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request)
    {
        List<TokenRevokedIntegrationEvent> eventsToPublish = [];

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var user = await authenticationService.AuthenticateWithGoogleAsync(request);

            //1. Xử lý device và session
            var device = await deviceManagementService.GetOrUpsertDeviceAsync(user.Id, request.ClientDeviceId!,
                request.DeviceType!.Value,
                request.DeviceToken);

            //2. Tạo token
            var (accessToken, jti, refreshToken) = await tokenService.GenerateTokensAsync(user);

            deviceManagementService.PrepareNewDeviceSession(device.Id, jti, refreshToken);

            var sessionsToRevoke =
                await deviceManagementService.GetOldestSessionsToRevokeAsync(user.Id, request.DeviceType.Value, device.Id, jti);
            if (sessionsToRevoke.Any())
            {
                deviceManagementService.MarkSessionsAsRevoked(sessionsToRevoke);

                // Chuẩn bị event để publish sau
                eventsToPublish.AddRange(sessionsToRevoke.Select(s =>
                    new TokenRevokedIntegrationEvent(s.AccessTokenId, s.RevokedAt!.Value)));
            }

            await dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return new LoginResponse(accessToken, refreshToken);
        }
        catch (Exception)
        {
            // 7. Nếu có bất kỳ lỗi nào xảy ra, rollback toàn bộ các thay đổi trong DB
            await transaction.RollbackAsync();

            // Ném lại exception để các middleware phía trên xử lý
            throw;
        }
    }

    public async Task<LoginResponse> RefreshAsync(TokenApiRequest request)
    {
        var principal = tokenService.GetPrincipalFromExpiredToken(request.Token)
                        ?? throw new BadRequestException("Access token không hợp lệ");

        var oldJwt = request.Token;

        var jti = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        // 1) Tìm device theo clientDeviceId 
        var device = await authDbContext.Devices
                         .FirstOrDefaultAsync(d => d.ClientDeviceId == request.ClientDeviceId)
                     ?? throw new NotFoundException("Thiết bị không hợp lệ");

        // 2) Tìm session hợp lệ theo device + refreshToken
        var session = await authDbContext.DeviceSessions.FirstOrDefaultAsync(s =>
            s.DeviceId == device.Id &&
            s.RefreshToken == request.RefreshToken &&
            !s.IsRevoked
            && s.AccessTokenId == jti
        );

        if (session is null)
            throw new BadRequestException("Session hoặc Refresh token không hợp lệ");

        // 4) Phát access token mới
        (string newJwtToken, string newJti, string newRefreshToken) = tokenService.RefreshToken(oldJwt);

        session.AccessTokenId = newJti;
        session.RefreshToken = newRefreshToken;
        session.LastRefeshToken = DateTimeOffset.UtcNow;

        await authDbContext.SaveChangesAsync();

        return new LoginResponse(newJwtToken, newRefreshToken);
    }


    public async Task<bool> RevokeAsync(TokenApiRequest request)
    {
        var subjectRef = actorAccessor.GetRequiredSubjectRef();

        var resolveResponse = await piiClient.ResolveUserIdBySubjectRefAsync(new ResolveUserIdBySubjectRefRequest
        {
            SubjectRef = subjectRef.ToString()
        });

        if (!Guid.TryParse(resolveResponse.UserId, out var userId))
        {
            return false;
        }

        var principal = tokenService.GetPrincipalFromExpiredToken(request.Token)
                        ?? throw new BadRequestException("Access token không hợp lệ");

        var jti = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        var device = await authDbContext.Devices
                         .FirstOrDefaultAsync(d => d.ClientDeviceId == request.ClientDeviceId && d.UserId == userId)
                     ?? throw new NotFoundException("Thiết bị không hợp lệ");

        var session = await authDbContext.DeviceSessions
                           .Include(ds => ds.Device)
                           .Where(s =>
                               s.DeviceId == device.Id &&
                               s.AccessTokenId == jti &&
                               s.RefreshToken == request.RefreshToken)
                           .FirstOrDefaultAsync() ??
                       throw new NotFoundException("Session không tồn tại hoặc đã bị thu hồi");

        await tokenRevocationService.RevokeSessionsAsync(new List<DeviceSession> { session });

        return true;
    }
}