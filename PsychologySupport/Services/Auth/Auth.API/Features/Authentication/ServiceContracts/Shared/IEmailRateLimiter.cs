namespace Auth.API.Features.Authentication.ServiceContracts.Shared;

public interface IEmailRateLimiter
{
    Task<bool> HasSentRecentlyAsync(string email);
    Task MarkAsSentAsync(string email);
}