using Post.Domain.Aggregates.Gifts.ValueObjects;
using Post.Domain.Aggregates.Posts.ValueObjects;
using Post.Domain.Exceptions;

namespace Post.Domain.Aggregates.Gifts;

public sealed class GiftAttach : AggregateRoot<Guid>, ISoftDeletable
{
    public GiftTarget Target { get; private set; } = null!;
    public GiftInfo Info { get; private set; } = null!;
    public AuthorInfo Sender { get; private set; } = null!;
    public long Amount { get; private set; }                 // điểm/đơn vị quy ước
    public string? Message { get; private set; }
    public DateTimeOffset SentAt { get; private set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedByAliasId { get; set; }

    // EF Core
    private GiftAttach() { }

    public static GiftAttach Create(
        string targetType, Guid targetId,
        Guid giftId,
        Guid senderAliasId, Guid? senderAliasVersionId,
        long amount,
        string? message = null)
    {
        if (amount <= 0)
            throw new InvalidGiftDataException("Giá trị quà tặng phải lớn hơn 0.");

        var attach = new GiftAttach
        {
            Id      = Guid.NewGuid(),
            Target  = GiftTarget.Create(targetType, targetId),
            Info    = GiftInfo.Create(giftId),
            Sender  = AuthorInfo.Create(senderAliasId, senderAliasVersionId),
            Amount  = amount,
            Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim(),
            SentAt  = DateTimeOffset.UtcNow
        };

        // attach.AddDomainEvent(new GiftAttachedEvent(...)); // nếu có
        return attach;
    }

    public void SoftDelete(Guid deleterAliasId)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByAliasId = deleterAliasId.ToString();
        // AddDomainEvent(new GiftAttachRemovedEvent(Id, ...));
    }
}
