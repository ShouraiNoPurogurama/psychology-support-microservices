using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Gifts;
using Post.Domain.Aggregates.Gifts.Enums;
using Post.Domain.Aggregates.Gifts.ValueObjects;
using Post.Domain.Aggregates.Posts.ValueObjects;

namespace Post.Application.Features.Gifts.Commands.AttachGift;

public sealed class AttachGiftCommandHandler : ICommandHandler<AttachGiftCommand, AttachGiftResult>
{
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryDbContext;
    private readonly IAliasVersionAccessor _aliasAccessor;
    private readonly ICurrentActorAccessor _currentActorAccessor;

    public AttachGiftCommandHandler(
        IPostDbContext context,
        IAliasVersionAccessor aliasAccessor,
        ICurrentActorAccessor currentActorAccessor,
        IQueryDbContext queryDbContext)
    {
        _context = context;
        _aliasAccessor = aliasAccessor;
        _currentActorAccessor = currentActorAccessor;
        _queryDbContext = queryDbContext;
    }

    public async Task<AttachGiftResult> Handle(AttachGiftCommand request, CancellationToken cancellationToken)
    {
        var aliasVersionId = await _aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(cancellationToken);
        var aliasId = _currentActorAccessor.GetRequiredAliasId();

        //Tạm thời cho rằng chỉ có thể tặng quà cho Post
        // Validate target exists
        var targetAuthorAliasId = await ValidateTargetExists(request.TargetType, request.TargetId, cancellationToken);

        // var isOwnedGift = await _queryDbContext.UserOwnedGiftReplicas.AsNoTracking()
        //     .AnyAsync(g => g.GiftId == request.GiftId &&
        //                    g.AliasId == aliasId, cancellationToken);
        //
        // if (!isOwnedGift)
        //     throw new ForbiddenException("Bạn chưa sở hữu quà tặng này. Vui lòng nạp lần đầu để mở khóa phần quà.");

        // Create gift attachment
        var target = GiftTarget.Create(request.TargetType.ToString(), request.TargetId);
        var sender = AuthorInfo.Create(aliasId, aliasVersionId);
        var giftInfo = GiftInfo.Create(request.GiftId);

        var giftAttach = GiftAttach.Create(
            target,
            targetAuthorAliasId,
            giftInfo.GiftId,
            sender.AliasId,
            sender.AliasVersionId,
            request.Quantity,
            request.Message
        );

        _context.GiftAttaches.Add(giftAttach);

        await _context.SaveChangesAsync(cancellationToken);

        return new AttachGiftResult(
            giftAttach.Id,
            request.TargetType,
            request.TargetId,
            request.GiftId,
            request.Message,
            request.Quantity,
            giftAttach.CreatedAt
        );
    }

    private async Task<Guid> ValidateTargetExists(GiftTargetType targetType, Guid targetId, CancellationToken cancellationToken)
    {
        var target = targetType switch
        {
            GiftTargetType.Post => await _context.Posts
                .Where(p => p.Id == targetId && !p.IsDeleted)
                .Select(p => p.Author.AliasId)
                .FirstOrDefaultAsync(cancellationToken),
            // GiftTargetType.Comment => await _context.Comments
            //     .Where(c => c.Id == targetId && !c.IsDeleted)
            //     .Select(c => c.Author.AliasId)
            //     .FirstOrDefaultAsync(cancellationToken),

            _ => throw new BadRequestException($"Invalid target type: {targetType}")
        };

        if (target != Guid.Empty)
        {
            return target;
        }

        var vietnameseTargetType = targetType switch
        {
            GiftTargetType.Post => "bài viết",
            // GiftTargetType.Comment => "bình luận",
            _ => "mục tiêu"
        };
        throw new NotFoundException($"Không tìm thấy {vietnameseTargetType} để gửi quà tặng.");
    }
}