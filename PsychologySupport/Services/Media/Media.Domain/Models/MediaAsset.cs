using Media.Domain.Enums;
using Media.Domain.Events;
using Media.Domain.Exceptions;
using Media.Domain.ValueObjects;

namespace Media.Domain.Models;

public sealed class MediaAsset : AggregateRoot<Guid>
{
    //VOs
    public MediaContent Content { get; private set; } = null!;
    public MediaChecksum Checksum { get; private set; } = null!;
    public MediaDimensions? Dimensions { get; private set; }
    /// <summary>
    /// Enum này trả lời câu hỏi: "Kết quả của việc kiểm duyệt là gì?". 
    /// </summary>
    public MediaModerationInfo Moderation { get; private set; } = null!;

    //Properties
    /// <summary>
    /// Enum này trả lời câu hỏi: "Media asset này đang ở giai đoạn nào trong vòng đời của nó?"
    /// </summary>
    public MediaState State { get; private set; }
    /// <summary>
    /// Enum này trả lời câu hỏi: "Media này được upload với mục đích gì?".
    /// Thuộc tính này là bất biến sau khi được tạo.
    /// </summary>
    public MediaPurpose Purpose { get; private set; } 
    public bool ExifRemoved { get; private set; } = false;
    public bool HoldThumbUntilPass { get; private set; } = false;

    //Collections
    private readonly List<MediaModerationAudit> _moderationAudits = new();
    private readonly List<MediaOwner> _owners = new();
    private readonly List<MediaProcessingJob> _processingJobs = new();
    private readonly List<MediaVariant> _variants = new();

    public IReadOnlyList<MediaModerationAudit> ModerationAudits => _moderationAudits.AsReadOnly();
    public IReadOnlyList<MediaOwner> Owners => _owners.AsReadOnly();
    public IReadOnlyList<MediaProcessingJob> ProcessingJobs => _processingJobs.AsReadOnly();
    public IReadOnlyList<MediaVariant> Variants => _variants.AsReadOnly();

    
    private static readonly Dictionary<MediaPurpose, MediaOwnerType[]> PurposeOwnerCompatibilityMap = new()
    {
        { MediaPurpose.PostAttachment, [MediaOwnerType.Post] },
        { MediaPurpose.CommentAttachment, [MediaOwnerType.Comment] },
        { MediaPurpose.Avatar, [MediaOwnerType.Profile] },
        { MediaPurpose.ProfileBackground, [MediaOwnerType.Profile] }
    };
    
    private MediaAsset() { }

    public static MediaAsset Create(
    Guid? seedMediaId,
    string mimeType,
    long sizeInBytes,
    string checksumSha256,
    MediaPurpose purpose,
    int? width = null,
    int? height = null,
    string? phash64 = null)
    {
        var newId = seedMediaId ?? Guid.NewGuid();

        var media = new MediaAsset
        {
            Id = newId,
            Purpose = purpose,
            Content = MediaContent.Create(mimeType, sizeInBytes, phash64),
            Checksum = MediaChecksum.CreateSha256(checksumSha256),
            Dimensions = MediaDimensions.CreateOptional(width, height),
            Moderation = MediaModerationInfo.Pending(),
            State = MediaState.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Tạo moderation audit mặc định (pending)
        var audit = MediaModerationAudit.Create(
            media.Id,
            MediaModerationStatus.Pending
        );
        media._moderationAudits.Add(audit);

        media.AddDomainEvent(new MediaUploadedEvent(
            media.Id,
            media.Purpose.ToString(),
            mimeType,
            sizeInBytes,
            checksumSha256,
            width,
            height,
            media.CreatedAt));

        return media;
    }

    // Business methods
    public void StartProcessing()
    {
        ValidateNotDeleted();
        
        if (State != MediaState.Processing)
        {
            var previousState = State;
            State = MediaState.Processing;
            
            AddDomainEvent(new MediaStateChangedEvent(Id, previousState.ToString(), State.ToString()));
        }
    }

    public void MarkAsReady()
    {
        ValidateNotDeleted();
        
        ValidateCanTransitionToReady();

        var previousState = State;
        State = MediaState.Ready;

        AddDomainEvent(new MediaStateChangedEvent(Id, previousState.ToString(), State.ToString()));
    }
    
    public void Hide(string reason)
    {
        ValidateNotDeleted();

        var previousState = State;
        State = MediaState.Hidden;

        AddDomainEvent(new MediaStateChangedEvent(Id, previousState.ToString(), State.ToString(), reason));
    }

    public void Block(string reason)
    {
        ValidateNotDeleted();

        var previousState = State;
        State = MediaState.Blocked;

        AddDomainEvent(new MediaStateChangedEvent(Id, previousState.ToString(), State.ToString(), reason));
    }

    public void Delete(string reason, string? deletedBy = null)
    {
        if (State == MediaState.Deleted) return;

        State = MediaState.Deleted;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;

        AddDomainEvent(new MediaDeletedEvent(Id, reason, DeletedAt.Value));
    }

    public void RequestModeration()
    {
        ValidateNotDeleted();

        ValidateModerationIsPending();

        State = MediaState.Moderating;

        AddDomainEvent(new MediaModerationRequestedEvent(Id));
    }

    public void ApproveModeration(string policyVersion, decimal? score = null, string? rawJson = null)
    {
        ValidateNotDeleted();

        ValidateModerationIsPending();
        
        Moderation = MediaModerationInfo.Approve(policyVersion, score, rawJson);

        //var audit = MediaModerationAudit.Create(Id, MediaModerationStatus.Approved, score, policyVersion, rawJson);
        //_moderationAudits.Add(audit);

        AddDomainEvent(new MediaModerationSucceededEvent(
            Id, 
            nameof(MediaModerationStatus.Approved), 
            score, 
            policyVersion, 
            DateTime.UtcNow));

        //Auto-mark as ready if processing is complete and moderation is approved
        if (State == MediaState.Moderating && CanTransitionToReady())
        {
            MarkAsReady();
        }
    }

    public void RejectModeration(string policyVersion, decimal? score = null, string? rawJson = null)
    {
        ValidateNotDeleted();
        
        ValidateModerationIsPending();
        
        Moderation = MediaModerationInfo.Reject(policyVersion, score, rawJson);

        //var audit = MediaModerationAudit.Create(Id, MediaModerationStatus.Rejected, score, policyVersion, rawJson);
        //_moderationAudits.Add(audit);

        AddDomainEvent(new MediaModerationSucceededEvent(
            Id, 
            MediaModerationStatus.Rejected.ToString(), 
            score, 
            policyVersion, 
            DateTime.UtcNow));

        //Auto-block rejected media
        Block("Moderation rejected");
    }

    /// <summary>
    /// Gắn media vào chủ sở hữu cuối cùng của nó (ví dụ: Post, Comment).
    /// Phương thức này xác thực các điều kiện nghiệp vụ trước khi gán quyền sở hữu.
    /// </summary>
    /// <param name="finalOwnerType">Loại chủ sở hữu cuối cùng (Post, Comment, ...)</param>
    /// <param name="finalOwnerId">ID của chủ sở hữu cuối cùng</param>
    /// <param name="initialOwnerId">ID của người dùng đã upload media, dùng để xác thực.</param>
    public void AttachToFinalOwner(MediaOwnerType finalOwnerType, Guid finalOwnerId, Guid initialOwnerId)
    {
        ValidateNotDeleted();
        
        if (_owners.All(o => o.MediaOwnerId != initialOwnerId))
        {
            throw new MediaOwnershipException(
                "The user attempting to attach the media is not the original uploader.");
        }

        if (State != MediaState.Ready && State != MediaState.Moderating && State != MediaState.Processing)
        {
            throw new MediaDomainException($"Media cannot be attached in its current state: {State}.");
        }

        // Kiểm tra sự tương thích giữa Purpose và OwnerType
        // Không thể gắn media có Purpose=Avatar vào một Post.
        if (!PurposeOwnerCompatibilityMap.TryGetValue(Purpose, out var validOwnerTypes) || !validOwnerTypes.Contains(finalOwnerType))
        {
            throw new MediaDomainException(
                $"Media with purpose '{Purpose}' cannot be attached to an owner of type '{finalOwnerType}'.");
        }

        AssignOwnership(finalOwnerType, finalOwnerId);
        
        SetStateAttached();

        AddDomainEvent(new MediaOwnershipAssignedEvent(Id, finalOwnerType.ToString(), finalOwnerId));
    }
    
    public void AssignOwnership(MediaOwnerType ownerType, Guid ownerId)
    {
        ValidateNotDeleted();

        if (_owners.Any(o => o.MediaOwnerType == ownerType && o.MediaOwnerId == ownerId))
            throw new MediaOwnershipException("Ownership already assigned to this owner.");

        var ownership = MediaOwner.Create(Id, ownerType, ownerId);
        _owners.Add(ownership);
    }
    

    public void AddProcessingJob(JobType jobType)
    {
        ValidateNotDeleted();

        if (_processingJobs.Any(j => j.JobType == jobType && j.Status != ProcessStatus.Failed && j.Status != ProcessStatus.Succeeded))
            throw new MediaProcessingException($"A {jobType} job is already queued or in progress.");

        var job = MediaProcessingJob.Create(Id, jobType);
        _processingJobs.Add(job);

        AddDomainEvent(new MediaProcessingStartedEvent(Id, jobType.ToString()));
    }

    public void AddVariant(VariantType variantType, MediaFormat format, int width, int height, long bytes, string bucketKey, string? cdnUrl = null)
    {
        ValidateNotDeleted();

        if (_variants.Any(v => v.VariantType == variantType))
            throw new MediaProcessingException($"A {variantType} variant already exists.");

        var variant = MediaVariant.Create(Id, variantType, format, width, height, bytes, bucketKey, cdnUrl);
        _variants.Add(variant);

        AddDomainEvent(new MediaVariantCreatedEvent(
            Id, 
            variant.Id, 
            variantType.ToString(), 
            format.ToString(), 
            width, 
            height, 
            cdnUrl ?? ""));
    }

    public void MarkExifRemoved()
    {
        ValidateNotDeleted();
        ExifRemoved = true;
    }

    public void SetHoldThumbUntilPass(bool hold)
    {
        ValidateNotDeleted();
        HoldThumbUntilPass = hold;
    }

    // Query methods
    public bool IsImage => Content.IsImage;
    public bool IsVideo => Content.IsVideo;
    public bool IsAudio => Content.IsAudio;
    public bool IsReady => State == MediaState.Ready;
    public bool IsProcessing => State == MediaState.Processing;
    public bool IsBlocked => State == MediaState.Blocked;
    public bool IsDeleted => State == MediaState.Deleted;
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public bool HasDimensions => Dimensions != null;
    public bool IsModerationApproved => Moderation.IsApproved;
    public bool RequiresModeration => Moderation.RequiresReview;

    
    
    
    public MediaVariant? GetOriginalVariant() 
        => _variants.FirstOrDefault(v => v.VariantType == VariantType.Original);

    public MediaVariant? GetThumbnailVariant() 
        => _variants.FirstOrDefault(v => v.VariantType == VariantType.Thumbnail);

    // Private validation methods
    private void ValidateNotDeleted()
    {
        if (IsDeleted)
            throw new MediaDomainException("Cannot perform operation on deleted media.");
    }
    
    private void SetStateAttached()
    {
        State = MediaState.Attached;
    }

    private void ValidateCanTransitionToReady()
    {
        if (!Moderation.IsApproved)
            throw new MediaDomainException("Cannot mark media as ready without approved moderation.");
    }

    private bool CanTransitionToReady()
        => Moderation.IsApproved;
    
    private void ValidateModerationIsPending()
    {
        if(!Moderation.IsPending) 
            throw new MediaModerationException("Media moderation has already been completed.");
    }
}

