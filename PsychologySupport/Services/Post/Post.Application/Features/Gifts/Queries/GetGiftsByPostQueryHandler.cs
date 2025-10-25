using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Application.Features.Gifts.Dtos;
using Post.Application.Features.Posts.Dtos;
using Post.Domain.Aggregates.Gifts;
using Post.Domain.Aggregates.Gifts.Enums;

namespace Post.Application.Features.Gifts.Queries;

internal sealed class GetGiftsByPostQueryHandler : IQueryHandler<GetGiftsByPostQuery, GetGiftsByPostResult>
{
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryDbContext;

    public GetGiftsByPostQueryHandler(IPostDbContext context, IQueryDbContext queryDbContext)
    {
        _context = context;
        _queryDbContext = queryDbContext;
    }

    public async Task<GetGiftsByPostResult> Handle(GetGiftsByPostQuery request, CancellationToken cancellationToken)
    {
        // --- BƯỚC 1: Lấy một trang các gift ---
        var giftsQuery = _context.GiftAttaches
            .AsNoTracking()
            .Where(g => g.Target.TargetId == request.PostId &&
                        g.Target.TargetType == nameof(GiftTargetType.Post) &&
                        !g.IsDeleted);

        // --- BƯỚC 2: Sắp xếp ---
        giftsQuery = request.SortBy?.ToLower() switch
        {
            _ => request.SortDescending
                ? giftsQuery.OrderByDescending(c => c.SentAt)
                : giftsQuery.OrderBy(c => c.SentAt)
        };

        var totalCount = await giftsQuery.CountAsync(cancellationToken);

        var gifts = await giftsQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // --- BƯỚC 3: Gộp và lấy dữ liệu tác giả (Sender) trong một batch ---
        var allAliasIds = gifts.Select(c => c.Sender.AliasId).Distinct().ToList();

        // Lấy thông tin author (người gửi)
        var authorMap = await _queryDbContext.AliasVersionReplica
            .AsNoTracking()
            .Where(a => allAliasIds.Contains(a.AliasId))
            .ToDictionaryAsync(a => a.AliasId, a => new AuthorDto(a.AliasId, a.Label, a.AvatarUrl), cancellationToken);

        // --- BƯỚC 4: Build DTO ---
        var giftDtos = gifts.Select(gift => MapToDto(gift, authorMap))
            .ToList();

        return new GetGiftsByPostResult(new PaginatedResult<GiftAttachDto>(
            request.Page,
            request.PageSize,
            totalCount,
            giftDtos
        ));
    }

    private GiftAttachDto MapToDto(
        GiftAttach gift,
        IReadOnlyDictionary<Guid, AuthorDto> authorMap)
    {
        var author = authorMap.GetValueOrDefault(gift.Sender.AliasId)
                     ?? new AuthorDto(gift.Sender.AliasId, "Người dùng ẩn", null);

        return new GiftAttachDto(
            gift.Info.GiftId, // Use Info.GiftId property
            gift.Target.TargetId, // PostId
            gift.Message,
            author,
            gift.SentAt, // Sử dụng SentAt thay vì CreatedAt
            gift.IsDeleted
        );
    }
}