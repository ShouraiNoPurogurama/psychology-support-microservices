using System.Collections.Concurrent;
using ChatBox.API.Domains.AIChats.Abstractions;

namespace ChatBox.API.Domains.AIChats.Services;

public class SessionConcurrencyManager : ISessionConcurrencyManager
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> SessionLocks = new();
    private static readonly ConcurrentDictionary<Guid, DateTime> ActiveSessions = new();
    private static readonly ConcurrentDictionary<Guid, Queue<string>> PendingMessagesBySession = new();
    
    private readonly ILogger<SessionConcurrencyManager> _logger;

    public SessionConcurrencyManager(ILogger<SessionConcurrencyManager> logger)
    {
        _logger = logger;
    }

    public async Task<bool> TryAcquireSessionLockAsync(Guid sessionId, TimeSpan timeout)
    {
        var sessionLock = SessionLocks.GetOrAdd(sessionId, _ => new SemaphoreSlim(1, 1));
        ActiveSessions.AddOrUpdate(sessionId, DateTime.UtcNow, (key, old) => DateTime.UtcNow);

        var acquired = await sessionLock.WaitAsync(timeout);
        if (!acquired)
        {
            _logger.LogWarning("Failed to acquire lock for session {SessionId}", sessionId);
        }

        return acquired;
    }

    public void ReleaseSessionLock(Guid sessionId)
    {
        if (SessionLocks.TryGetValue(sessionId, out var semaphore))
        {
            semaphore.Release();
        }
    }

    public bool ShouldThrottleMessage(Guid sessionId, string message)
    {
        var inputNormalized = message.Trim().ToLowerInvariant();
        var pendingQueue = PendingMessagesBySession.GetOrAdd(sessionId, _ => new Queue<string>());

        lock (pendingQueue)
        {
            if (pendingQueue.Count >= 2)
                return true;

            if (pendingQueue.Contains(inputNormalized))
                return true;

            return false;
        }
    }

    public void TrackPendingMessage(Guid sessionId, string message)
    {
        var inputNormalized = message.Trim().ToLowerInvariant();
        var pendingQueue = PendingMessagesBySession.GetOrAdd(sessionId, _ => new Queue<string>());

        lock (pendingQueue)
        {
            pendingQueue.Enqueue(inputNormalized);
        }
    }

    public void CompletePendingMessage(Guid sessionId, string message)
    {
        var pendingQueue = PendingMessagesBySession.GetOrAdd(sessionId, _ => new Queue<string>());

        lock (pendingQueue)
        {
            if (pendingQueue.Count > 0)
                pendingQueue.Dequeue();

            if (pendingQueue.Count == 0)
                PendingMessagesBySession.TryRemove(sessionId, out _);
        }
    }
}