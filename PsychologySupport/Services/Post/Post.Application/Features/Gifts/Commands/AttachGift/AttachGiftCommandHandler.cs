using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Gifts;
using Post.Domain.Aggregates.Gifts.DomainEvents;
using Post.Domain.Aggregates.Gifts.Enums;
using Post.Domain.Aggregates.Gifts.ValueObjects;
using Post.Domain.Aggregates.Posts.ValueObjects;

namespace Post.Application.Features.Gifts.Commands.AttachGift;

public sealed class AttachGiftCommandHandler : ICommandHandler<AttachGiftCommand, AttachGiftResult>
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionAccessor _aliasAccessor;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public AttachGiftCommandHandler(
        IPostDbContext context,
        IAliasVersionAccessor aliasAccessor,
        IOutboxWriter outboxWriter, ICurrentActorAccessor currentActorAccessor)
    {
        _context = context;
        _aliasAccessor = aliasAccessor;
        _outboxWriter = outboxWriter;
        _currentActorAccessor = currentActorAccessor;
    }

    public async Task<AttachGiftResult> Handle(AttachGiftCommand request, CancellationToken cancellationToken)
    {
        var aliasVersionId = await _aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(cancellationToken);

        // Validate target exists
        await ValidateTargetExists(request.TargetType, request.TargetId, cancellationToken);

        // Create gift attachment
        var target = GiftTarget.Create(request.TargetType.ToString(), request.TargetId);
        var sender = AuthorInfo.Create(_currentActorAccessor.GetRequiredAliasId(), aliasVersionId);
        var giftInfo = GiftInfo.Create(request.GiftId);

        var giftAttach = GiftAttach.Create(
            request.TargetType.ToString(),
            request.TargetId,
            giftInfo.GiftId,
            sender.AliasId,
            sender.AliasVersionId,
            1,
            request.Message
        );

        _context.GiftAttaches.Add(giftAttach);

        // Add domain event
        var giftAttachedEvent = new GiftAttachedEvent(
            target.TargetId,
            giftInfo.GiftId,
            sender.AliasId,
            request.Quantity,
            request.Message
        );
        await _outboxWriter.WriteAsync(giftAttachedEvent, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new AttachGiftResult(
            giftAttach.Id,
            request.TargetType,
            request.TargetId,
            request.GiftId,
            request.Message,
            giftAttach.CreatedAt
        );
    }

    private async Task ValidateTargetExists(GiftTargetType targetType, Guid targetId, CancellationToken cancellationToken)
    {
        bool exists = targetType switch
        {
            GiftTargetType.Post => await _context.Posts.AnyAsync(p => p.Id == targetId && !p.IsDeleted, cancellationToken),
            GiftTargetType.Comment => await _context.Comments.AnyAsync(c => c.Id == targetId && !c.IsDeleted, cancellationToken),
            _ => throw new BadRequestException($"Invalid target type: {targetType}")
        };

        if (!exists)
        {
            throw new NotFoundException($"{targetType} with ID {targetId} not found");
        }
    }
}
