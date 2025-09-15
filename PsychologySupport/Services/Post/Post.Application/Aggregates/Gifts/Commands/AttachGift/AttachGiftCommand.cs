using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Gifts.Commands.AttachGift;

public record AttachGiftCommand(
    string TargetType, // "post" or "comment"
    Guid TargetId,
    Guid GiftId,
    int Quantity,
    string? Message
) : ICommand<AttachGiftResult>;

public record AttachGiftResult(
    Guid GiftAttachId,
    string TargetType,
    Guid TargetId,
    Guid GiftId,
    string? Message,
    DateTimeOffset CreatedAt
);
