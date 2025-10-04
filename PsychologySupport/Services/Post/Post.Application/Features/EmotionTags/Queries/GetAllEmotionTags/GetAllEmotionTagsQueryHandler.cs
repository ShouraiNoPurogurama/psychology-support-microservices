using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Application.Features.EmotionTags.Dtos;

namespace Post.Application.Features.EmotionTags.Queries.GetAllEmotionTags;

internal sealed class GetAllEmotionTagsQueryHandler : IQueryHandler<GetAllEmotionTagsQuery, GetAllEmotionTagsResult>
{
    private readonly IQueryDbContext _queryContext;

    public GetAllEmotionTagsQueryHandler(IQueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<GetAllEmotionTagsResult> Handle(GetAllEmotionTagsQuery request, CancellationToken cancellationToken)
    {
        var emotionTagsQuery = _queryContext.EmotionTagReplicas.AsNoTracking();

        if (request.IsActive.HasValue)
        {
            emotionTagsQuery = emotionTagsQuery.Where(et => et.IsActive == request.IsActive.Value);
        }

        // Get all emotion tags
        var emotionTags = await emotionTagsQuery
            .OrderBy(et => et.DisplayName)
            .ToListAsync(cancellationToken);

        // If AliasId is provided, check ownership for each tag
        IEnumerable<Guid> ownedTagIds = Enumerable.Empty<Guid>();
        if (request.AliasId.HasValue)
        {
            ownedTagIds = await _queryContext.UserOwnedTagReplicas
                .AsNoTracking()
                .Where(uot => uot.AliasId == request.AliasId.Value)
                .Select(uot => uot.EmotionTagId)
                .ToListAsync(cancellationToken);
        }

        var ownedTagIdsSet = new HashSet<Guid>(ownedTagIds);

        var result = emotionTags.Select(et => new EmotionTagDto(
            et.Id,
            et.Code,
            et.DisplayName,
            et.MediaId,
            et.IsActive,
            request.AliasId.HasValue && ownedTagIdsSet.Contains(et.Id)
        )).ToList();

        return new GetAllEmotionTagsResult(result);
    }
}
