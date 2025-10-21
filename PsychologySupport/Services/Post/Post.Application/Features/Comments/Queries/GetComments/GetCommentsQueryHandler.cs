using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Features.Comments.Dtos;
using Post.Application.Features.Posts.Dtos;
using Post.Domain.Aggregates.Comments;
using Post.Domain.Aggregates.Reaction.Enums;

namespace Post.Application.Features.Comments.Queries.GetComments;

internal sealed class GetCommentsQueryHandler : IQueryHandler<GetCommentsQuery, GetCommentsResult>
{
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryDbContext;
    private readonly ICurrentActorAccessor _actorAccessor;
    private const int PREVIEW_REPLY_COUNT = 2; // Số lượng reply xem trước

    public GetCommentsQueryHandler(IPostDbContext context, IQueryDbContext queryDbContext, ICurrentActorAccessor actorAccessor)
    {
        _context = context;
        _queryDbContext = queryDbContext;
        _actorAccessor = actorAccessor;
    }

    public async Task<GetCommentsResult> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        var aliasId = _actorAccessor.GetRequiredAliasId();

        // --- BƯỚC 1: Lấy một trang các comment gốc (root comments) ---
        var rootCommentsQuery = _context.Comments
            .AsNoTracking()
            .Where(c => c.PostId == request.PostId && !c.IsDeleted && c.Hierarchy.ParentCommentId == null);

        rootCommentsQuery = request.SortBy?.ToLower() switch
        {
            "createdat" => request.SortDescending
                ? rootCommentsQuery.OrderByDescending(c => c.CreatedAt)
                : rootCommentsQuery.OrderBy(c => c.CreatedAt),
            _ => rootCommentsQuery.OrderBy(c => c.CreatedAt)
        };

        var totalCount = await rootCommentsQuery.CountAsync(cancellationToken);

        var rootComments = await rootCommentsQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        if (!rootComments.Any())
        {
            return new GetCommentsResult(PaginatedResult<CommentSummaryDto>.Empty(request.Page, request.PageSize));
        }

        // --- BƯỚC 2: Lấy các replies xem trước (preview replies) 
        var rootCommentIds = rootComments.Select(c => c.Id).ToList();

        var previewReplies = await _context.Comments
            .AsNoTracking()
            .Where(parentComment => rootCommentIds.Contains(parentComment.Id))
            .SelectMany(parentComment =>
                _context.Comments
                    .AsNoTracking()
                    .Where(reply => reply.Hierarchy.ParentCommentId == parentComment.Id && !reply.IsDeleted)
                    .OrderBy(reply => reply.CreatedAt)
                    .Take(PREVIEW_REPLY_COUNT)
            )
            .ToListAsync(cancellationToken);

        // --- BƯỚC 3: Gộp và lấy dữ liệu phụ thuộc trong một batch ---
        var allCommentsInScope = rootComments.Concat(previewReplies).ToList();
        var allCommentIds = allCommentsInScope.Select(c => c.Id).ToHashSet();
        var allAliasIds = allCommentsInScope.Select(c => c.Author.AliasId).Distinct().ToList();

        // Lấy thông tin reaction
        var reactedCommentIds = await _context.Reactions
            .AsNoTracking()
            .Where(r => r.Target.TargetType == ReactionTargetType.Comment &&
                        allCommentIds.Contains(r.Target.TargetId) &&
                        r.Author.AliasId == aliasId &&
                        !r.IsDeleted)
            .Select(r => r.Target.TargetId)
            .ToHashSetAsync(cancellationToken);

        // Lấy thông tin author
        var authorMap = await _queryDbContext.AliasVersionReplica
            .AsNoTracking()
            .Where(a => allAliasIds.Contains(a.AliasId))
            .ToDictionaryAsync(a => a.AliasId, a => new AuthorDto(a.AliasId, a.Label, a.AvatarUrl), cancellationToken);

        // --- BƯỚC 4: Build DTO với cấu trúc hybrid ---
        var repliesLookup = previewReplies.ToLookup(r => r.Hierarchy.ParentCommentId);

        var commentDtos = rootComments.Select(root =>
            {
                var repliesForThisRoot = repliesLookup[root.Id]
                    .Select(reply => MapToDto(reply, authorMap, reactedCommentIds, []))
                    .ToList();

                return MapToDto(root, authorMap, reactedCommentIds, repliesForThisRoot);
            })
            .ToList();

        return new GetCommentsResult(new PaginatedResult<CommentSummaryDto>(
            request.Page,
            request.PageSize,
            totalCount,
            commentDtos
        ));
    }

    // Hàm helper để map từ Comment entity sang CommentSummaryDto
    private CommentSummaryDto MapToDto(
        Comment comment,
        IReadOnlyDictionary<Guid, AuthorDto> authorMap,
        IReadOnlySet<Guid> reactedCommentIds,
        List<CommentSummaryDto> replies) // Nhận danh sách replies đã được map
    {
        var author = authorMap.GetValueOrDefault(comment.Author.AliasId)
                     ?? new AuthorDto(comment.Author.AliasId, "Người dùng ẩn", null);

        var isReacted = reactedCommentIds.Contains(comment.Id);

        return new CommentSummaryDto(
            comment.Id,
            comment.PostId,
            comment.Content.Value,
            isReacted,
            author,
            new HierarchyDto(
                comment.Hierarchy.ParentCommentId,
                comment.Hierarchy.Path,
                comment.Hierarchy.Level
            ),
            replies, // Gán danh sách replies xem trước
            comment.CreatedAt,
            comment.EditedAt,
            comment.ReactionCount,
            comment.ReplyCount,
            comment.IsDeleted
        );
    }
}