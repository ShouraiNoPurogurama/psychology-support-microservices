using Media.Domain.Enums;

namespace Media.Domain.Models;

public sealed class MediaOwner : AuditableEntity<Guid>
{
    public Guid MediaId { get; private set; }
    public MediaOwnerType MediaOwnerType { get; private set; }
    public Guid MediaOwnerId { get; private set; }

    public MediaAsset Media { get; private set; } = null!;

    // EF Core constructor
    private MediaOwner() { }

    // Factory method
    public static MediaOwner Create(Guid mediaId, MediaOwnerType ownerType, Guid ownerId)
    {
        return new MediaOwner
        {
            Id = Guid.NewGuid(),
            MediaId = mediaId,
            MediaOwnerType = ownerType,
            MediaOwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
