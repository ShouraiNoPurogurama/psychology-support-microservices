using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Application.Features.Comments.Dtos;
using Post.Application.Features.Posts.Dtos;
using Post.Application.ReadModels.Models;
using Post.Domain.Aggregates.Comments;

namespace Post.Application.Features.Comments.Queries.GetComments;

internal sealed class GetCommentsQueryHandler : IQueryHandler<GetCommentsQuery, PaginatedResult<CommentDto>>
{
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryDbContext; // Tốt!

    public GetCommentsQueryHandler(IPostDbContext context, IQueryDbContext queryDbContext)
    {
        _context = context;
        _queryDbContext = queryDbContext;
    }

    public async Task<PaginatedResult<CommentDto>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        // Build query như cũ
        var query = _context.Comments
            .Where(c => c.PostId == request.PostId && !c.IsDeleted)
            .AsQueryable();

        if (request.ParentCommentId.HasValue)
        {
            query = query.Where(c => c.Hierarchy.ParentCommentId == request.ParentCommentId.Value);
        }
        else
        {
            query = query.Where(c => c.Hierarchy.ParentCommentId == null);
        }

        query = request.SortBy?.ToLower() switch
        {
            "createdat" => request.SortDescending
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderBy(c => c.CreatedAt)
        };
        
        var totalCount = await query.CountAsync(cancellationToken);

        // BƯỚC 1.1: Lấy trang comment gốc
        var pagedComments = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // BƯỚC 1.2: Lấy TẤT CẢ replies (dùng UNION, không cần thư viện)
        var baseRepliesQuery = _context.Comments
            .Where(c => !c.IsDeleted && c.PostId == request.PostId);

        // Tạo một IQueryable rỗng để bắt đầu
        var allRepliesQuery = baseRepliesQuery
            .Where(c => false); // (c => c.Id == Guid.Empty)

        foreach (var comment in pagedComments)
        {
            // Điều chỉnh format của Path nếu cần
            var pathFilter = comment.Hierarchy.Path + comment.Id.ToString() + "/"; 
            var repliesForThisPath = baseRepliesQuery
                .Where(c => c.Hierarchy.Path.StartsWith(pathFilter));
            
            // Nối các query lại với nhau bằng UNION
            allRepliesQuery = allRepliesQuery.Union(repliesForThisPath);
        }

        var allReplies = await allRepliesQuery
            .ToListAsync(cancellationToken);

        // BƯỚC 1.3: Gộp tất cả comment (gốc + replies)
        var allComments = pagedComments.Concat(allReplies)
                                     .DistinctBy(c => c.Id)
                                     .ToList();

        // BƯỚC 2: Query AliasVersionReplica MỘT LẦN DUY NHẤT
        var allAliasIds = allComments.Select(c => c.Author.AliasId).Distinct().ToList();

        // Giả sử AliasVersionReplica có các trường: AliasId, DisplayName, AvatarUrl
        var authorMap = await _queryDbContext.AliasVersionReplica
            .Where(a => allAliasIds.Contains(a.AliasId))
            // Key là AliasId, Value là chính object replica
            .ToDictionaryAsync(a => a.AliasId, cancellationToken); 

        // BƯỚC 3: Build cây DTO (in-memory)
        var allCommentsLookup = allComments.ToLookup(c => c.Hierarchy.ParentCommentId);
        var commentDtos = new List<CommentDto>();

        // Fallback Author DTO (phòng trường hợp data replica bị trễ)
        var defaultAuthor = (Guid aliasId) => new AuthorDto(aliasId, "Người dùng ẩn", "default_avatar.png");

        //TODO lấy avatarUrl từ authorMap nếu có
        string? avatarUrl = null;
        
        // Lặp qua trang gốc
        foreach (var comment in pagedComments)
        {
            // Lấy author từ map
            var author = authorMap.GetValueOrDefault(comment.Author.AliasId, null);
            var authorDto = author != null 
                ? new AuthorDto(author.AliasId, author.Label, avatarUrl) // <-- Mapping chuẩn
                : defaultAuthor(comment.Author.AliasId);

            // Gọi hàm helper MỚI (đồng bộ, in-memory)
            var replies = BuildReplyDtos(comment.Id, allCommentsLookup, authorMap, defaultAuthor);

            commentDtos.Add(new CommentDto(
                comment.Id,
                comment.PostId,
                comment.Content.Value,
                authorDto, // <-- Dùng DTO đã map
                new HierarchyDto(
                    comment.Hierarchy.ParentCommentId,
                    comment.Hierarchy.Path,
                    comment.Hierarchy.Level
                ),
                comment.CreatedAt,
                comment.EditedAt,
                comment.ReactionCount,
                comment.ReplyCount,
                comment.IsDeleted
            ));
        }

        return new PaginatedResult<CommentDto>(
            request.Page,
            request.PageSize,
            totalCount,
            commentDtos
        );
    }

    // XÓA HÀM GetRepliesRecursively (async) CŨ
    
    // THÊM HÀM HELPER (private, synchronous) NÀY
    private List<CommentDto> BuildReplyDtos(
        Guid parentId,
        ILookup<Guid?, Comment> commentLookup,
        IReadOnlyDictionary<Guid, AliasVersionReplica> authorMap, // Hoặc DTO replica của bạn
        Func<Guid, AuthorDto> defaultAuthorFactory)
    {
        var replyDtos = new List<CommentDto>();

        string ?avatar = null; // TODO: Lấy avatar từ authorMap nếu có
        
        // Lấy replies từ lookup (đã có sẵn trong memory)
        foreach (var reply in commentLookup[parentId].OrderBy(c => c.CreatedAt))
        {
            // Đệ quy in-memory (rất nhanh)
            var nestedReplies = BuildReplyDtos(reply.Id, commentLookup, authorMap, defaultAuthorFactory);
            
            var author = authorMap.GetValueOrDefault(reply.Author.AliasId, null);
            var authorDto = author != null
                ? new AuthorDto(author.AliasId, author.Label, avatar) // <-- Mapping chuẩn
                : defaultAuthorFactory(reply.Author.AliasId);

            replyDtos.Add(new CommentDto(
                reply.Id,
                reply.PostId,
                reply.Content.Value,
                authorDto, // <-- Dùng DTO đã map
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
            ));
        }

        return replyDtos;
    }
}