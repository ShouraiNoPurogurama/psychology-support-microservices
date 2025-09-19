using Media.Domain.Enums;
using Media.Domain.Exceptions;

namespace Media.Domain.Models;

public sealed class MediaProcessingJob : AuditableEntity<Guid>
{
    public Guid MediaId { get; private set; }
    public JobType JobType { get; private set; }
    public ProcessStatus Status { get; private set; }
    public int Attempt { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    public MediaAsset Media { get; private set; } = null!;

    // EF Core constructor
    private MediaProcessingJob() { }

    // Factory method
    public static MediaProcessingJob Create(Guid mediaId, JobType jobType)
    {
        return new MediaProcessingJob
        {
            Id = Guid.NewGuid(),
            MediaId = mediaId,
            JobType = jobType,
            Status = ProcessStatus.Queued,
            Attempt = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Business methods
    public void Start()
    {
        if (Status != ProcessStatus.Queued)
            throw new MediaProcessingException($"Cannot start job in {Status} status.");

        Status = ProcessStatus.Running;
        Attempt++;
        NextRetryAt = null;
    }

    public void Complete()
    {
        if (Status != ProcessStatus.Running)
            throw new MediaProcessingException($"Cannot complete job in {Status} status.");

        Status = ProcessStatus.Succeeded;
        ErrorMessage = null;
        NextRetryAt = null;
    }

    public void Fail(string errorMessage, TimeSpan? retryDelay = null)
    {
        if (Status != ProcessStatus.Running)
            throw new MediaProcessingException($"Cannot fail job in {Status} status.");

        ErrorMessage = errorMessage;

        if (Attempt >= MaxRetryAttempts)
        {
            Status = ProcessStatus.Failed;
            NextRetryAt = null;
        }
        else
        {
            Status = ProcessStatus.Queued;
            NextRetryAt = DateTime.UtcNow.Add(retryDelay ?? CalculateRetryDelay());
        }
    }

    public void Cancel()
    {
        if (Status == ProcessStatus.Succeeded || Status == ProcessStatus.Failed)
            throw new MediaProcessingException($"Cannot cancel job in {Status} status.");

        Status = ProcessStatus.Cancelled;
        NextRetryAt = null;
    }

    public bool CanRetry => Status == ProcessStatus.Queued &&
                           Attempt < MaxRetryAttempts &&
                           (NextRetryAt == null || NextRetryAt <= DateTime.UtcNow);

    public bool IsFinished => Status == ProcessStatus.Succeeded ||
                             Status == ProcessStatus.Failed ||
                             Status == ProcessStatus.Cancelled;

    private const int MaxRetryAttempts = 3;

    private TimeSpan CalculateRetryDelay()
    {
        // Exponential backoff: 2^attempt minutes
        var delayMinutes = Math.Pow(2, Attempt);
        return TimeSpan.FromMinutes(Math.Min(delayMinutes, 60)); // Cap at 1 hour
    }
}
