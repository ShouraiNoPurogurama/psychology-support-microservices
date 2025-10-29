using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Features.Comments.Dtos;
using Post.Application.Features.Posts.Dtos;
using Post.Domain.Aggregates.Reaction.Enums;

namespace Post.Application.Features.Comments.Queries.GetCommentReplies;

internal sealed class GetCommentRepliesQueryHandler : IQueryHandler<GetCommentRepliesQuery, GetCommentRepliesResult>
{
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryDbContext;
    private readonly ICurrentActorAccessor _actorAccessor;

    public GetCommentRepliesQueryHandler(
        IPostDbContext context,
        IQueryDbContext queryDbContext,
        ICurrentActorAccessor actorAccessor)
    {
        _context = context;
        _queryDbContext = queryDbContext;
        _actorAccessor = actorAccessor;
    }

    public async Task<GetCommentRepliesResult> Handle(GetCommentRepliesQuery request, CancellationToken cancellationToken)
    {
        // 1) Verify parent comment exists (and not deleted)
        var parentCommentExists = await _context.Comments
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ParentCommentId && !c.IsDeleted, cancellationToken);

        if (!parentCommentExists)
            throw new NotFoundException("Không tìm thấy bình luận cha.", "PARENT_COMMENT_NOT_FOUND");

        // 2) Base query for replies under the parent (not deleted)
        var baseQuery = _context.Comments
            .AsNoTracking()
            .Where(c => c.Hierarchy.ParentCommentId == request.ParentCommentId && !c.IsDeleted)
            .OrderBy(c => c.Hierarchy.Level)
            ;

        // Sorting (giữ giống convention: mặc định theo CreatedAt asc)
        baseQuery = baseQuery.OrderBy(c => c.CreatedAt);

        // 3) Total count (trước phân trang)
        var totalCount = await baseQuery.CountAsync(cancellationToken);

        // 4) Page fetch entities (KHÔNG project sớm để còn batch author + reacted)
        var pageReplies = await baseQuery
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Không có replies -> trả về rỗng
        if (pageReplies.Count == 0)
        {
            var empty = new PaginatedResult<ReplySummaryDto>(request.PageIndex, request.PageSize, totalCount, []);
            return new GetCommentRepliesResult(empty);
        }

        // 5) Batch-dependent data
        var currentAliasId = _actorAccessor.GetRequiredAliasId();

        var replyIds = pageReplies.Select(r => r.Id).ToHashSet();
        var authorAliasIds = pageReplies.Select(r => r.Author.AliasId).Distinct().ToList();

        // 5.1) Lấy map author từ read-model (replica)
        var authorMap = await _queryDbContext.AliasVersionReplica
            .AsNoTracking()
            .Where(a => authorAliasIds.Contains(a.AliasId))
            .ToDictionaryAsync(
                a => a.AliasId,
                a => new AuthorDto(a.AliasId, a.Label, a.AvatarUrl),
                cancellationToken);

        // 5.2) Lấy các comment mà current user đã react
        var reactedIds = await _context.Reactions
            .AsNoTracking()
            .Where(r => r.Target.TargetType == ReactionTargetType.Comment
                        && replyIds.Contains(r.Target.TargetId)
                        && r.Author.AliasId == currentAliasId
                        && !r.IsDeleted)
            .Select(r => r.Target.TargetId)
            .ToHashSetAsync(cancellationToken);

        // 6) Map sang DTO
        var dtos = pageReplies
            .OrderBy(r => r.CreatedAt)
            .Select(reply =>
            {
                var author = authorMap.GetValueOrDefault(reply.Author.AliasId)
                             ?? new AuthorDto(reply.Author.AliasId, "Người dùng ẩn", null);

                var isReacted = reactedIds.Contains(reply.Id);

                return new ReplySummaryDto(
                    reply.Id,
                    reply.PostId,
                    reply.Content.Value,
                    isReacted,
                    author,
                    new HierarchyDto(
                        reply.Hierarchy.ParentCommentId,
                        reply.Hierarchy.Path,
                        reply.Hierarchy.Level
                    ),
                    reply.CreatedAt,
                    reply.EditedAt,
                    reply.ReactionCount,
                    reply.ReplyCount,
                    reply.IsDeleted
                );
            })
            .ToList();

        // 7) Wrap vào PaginatedResult
        var paginated = new PaginatedResult<ReplySummaryDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            dtos
        );

        return new GetCommentRepliesResult(paginated);
    }
}
