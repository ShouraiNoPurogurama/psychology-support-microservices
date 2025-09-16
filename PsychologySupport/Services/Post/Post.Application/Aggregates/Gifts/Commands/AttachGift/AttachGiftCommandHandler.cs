using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Microsoft.EntityFrameworkCore;
using Post.Domain.Aggregates.Gifts;
using Post.Domain.Aggregates.Gifts.DomainEvents;
using Post.Domain.Aggregates.Gifts.ValueObjects;
using Post.Domain.Aggregates.Posts.ValueObjects;

namespace Post.Application.Aggregates.Gifts.Commands.AttachGift;

internal sealed class AttachGiftCommandHandler : ICommandHandler<AttachGiftCommand, AttachGiftResult>
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionResolver _aliasResolver;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public AttachGiftCommandHandler(
        IPostDbContext context,
        IAliasVersionResolver aliasResolver,
        IOutboxWriter outboxWriter, IActorResolver actorResolver)
    {
        _context = context;
        _aliasResolver = aliasResolver;
        _outboxWriter = outboxWriter;
        _actorResolver = actorResolver;
    }

    public async Task<AttachGiftResult> Handle(AttachGiftCommand request, CancellationToken cancellationToken)
    {
        var aliasVersionId = await _aliasResolver.GetCurrentAliasVersionIdAsync(cancellationToken);

        // Validate target exists
        await ValidateTargetExists(request.TargetType, request.TargetId, cancellationToken);

        // Create gift attachment
        var giftAttachId = Guid.NewGuid();
        var target = GiftTarget.Create(request.TargetType, request.TargetId);
        var sender = AuthorInfo.Create(_actorResolver.AliasId, aliasVersionId);
        var giftInfo = GiftInfo.Create(request.GiftId);

        var giftAttach = GiftAttach.Create(
            request.TargetType,
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

    private async Task ValidateTargetExists(string targetType, Guid targetId, CancellationToken cancellationToken)
    {
        bool exists = targetType.ToLower() switch
        {
            "post" => await _context.Posts.AnyAsync(p => p.Id == targetId && !p.IsDeleted, cancellationToken),
            "comment" => await _context.Comments.AnyAsync(c => c.Id == targetId && !c.IsDeleted, cancellationToken),
            _ => throw new BadRequestException($"Invalid target type: {targetType}")
        };

        if (!exists)
        {
            throw new NotFoundException($"{targetType} with ID {targetId} not found");
        }
    }
}
