namespace Auth.API.Features.Authentication.ServiceContracts.Shared;

public interface ITokenRevocationService
{
    Task RevokeSessionsAsync(IEnumerable<DeviceSession> sessions);
}