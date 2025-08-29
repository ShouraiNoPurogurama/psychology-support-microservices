namespace ChatBox.API.Domains.AIChats.Abstractions;

public interface ISessionConcurrencyManager
{
    Task<bool> TryAcquireSessionLockAsync(Guid sessionId, TimeSpan timeout);
    void ReleaseSessionLock(Guid sessionId);
    bool ShouldThrottleMessage(Guid sessionId, string message);
    void TrackPendingMessage(Guid sessionId, string message);
    void CompletePendingMessage(Guid sessionId, string message);
}