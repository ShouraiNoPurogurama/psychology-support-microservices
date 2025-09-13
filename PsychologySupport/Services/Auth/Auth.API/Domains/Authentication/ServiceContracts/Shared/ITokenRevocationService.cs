namespace Auth.API.Domains.Authentication.ServiceContracts.Shared;

public interface ITokenRevocationService
{
    Task RevokeSessionsAsync(IEnumerable<DeviceSession> sessions);
}