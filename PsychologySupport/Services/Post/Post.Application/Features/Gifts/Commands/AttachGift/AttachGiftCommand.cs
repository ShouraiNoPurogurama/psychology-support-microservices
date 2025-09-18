using BuildingBlocks.CQRS;
using Post.Domain.Aggregates.Gifts.Enums;

namespace Post.Application.Features.Gifts.Commands.AttachGift;

public record AttachGiftCommand(
    GiftTargetType TargetType,
    Guid TargetId,
    Guid GiftId,
    int Quantity,
    string? Message
) : ICommand<AttachGiftResult>;

public record AttachGiftResult(
    Guid GiftAttachId,
    GiftTargetType TargetType,
    Guid TargetId,
    Guid GiftId,
    string? Message,
    DateTimeOffset CreatedAt
);
