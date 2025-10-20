using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Application.Features.Reactions.Dtos;
using Post.Domain.Aggregates.Reaction.Enums;

namespace Post.Application.Features.Reactions.Queries.GetReactionsForPost;

internal sealed class GetReactionsForPostQueryHandler : IQueryHandler<GetReactionsForPostQuery, GetReactionsForPostResult>
{
    private readonly IPostDbContext _postDb;
    private readonly IQueryDbContext _queryDb;

    public GetReactionsForPostQueryHandler(IPostDbContext postDb, IQueryDbContext queryDb)
    {
        _postDb = postDb;
        _queryDb = queryDb;
    }

    public async Task<GetReactionsForPostResult> Handle(GetReactionsForPostQuery request, CancellationToken cancellationToken)
    {
        // Ensure post exists (source of truth)
        var exists = await _postDb.Posts
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (!exists)
            throw new NotFoundException("Post not found or deleted", "POST_NOT_FOUND");

        var baseQuery = _postDb.Reactions
            .AsNoTracking()
            .Where(r => r.Target.TargetId == request.PostId &&
                        r.Target.TargetType == ReactionTargetType.Post &&
                        !r.IsDeleted);

        var total = await baseQuery.CountAsync(cancellationToken);

        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.Size <= 0 ? 10 : request.Size;

        var slice = await baseQuery
            .OrderByDescending(r => r.ReactedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(r => new
            {
                ReactionId = r.Id,
                PostId = r.Target.TargetId,
                AliasId = r.Author.AliasId,
                ReactionCode = r.Type.Code,
                CreatedAt = r.ReactedAt
            })
            .ToListAsync(cancellationToken);

        var aliasIds = slice.Select(s => s.AliasId).Distinct().ToList();
        
        var aliasDisplayLookup = await _queryDb.AliasVersionReplica
            .AsNoTracking()
            .Where(a => aliasIds.Contains(a.AliasId))
            .Select(a => new { a.AliasId, a.Label })
            .ToDictionaryAsync(a => a.AliasId, a => a.Label, cancellationToken);

        var dtos = slice.Select(s => new ReactionDto(
                s.ReactionId,
                s.PostId,
                s.AliasId,
                aliasDisplayLookup.TryGetValue(s.AliasId, out var name) ? name : "Unknown",
                s.ReactionCode,
                s.CreatedAt
            ))
            .ToList();

        var paged = new PaginatedResult<ReactionDto>(page, size, total, dtos);
        return new GetReactionsForPostResult(paged);
    }
}