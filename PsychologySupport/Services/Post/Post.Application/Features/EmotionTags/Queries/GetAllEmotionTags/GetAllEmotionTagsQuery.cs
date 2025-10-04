using BuildingBlocks.CQRS;
using Post.Application.Features.EmotionTags.Dtos;

namespace Post.Application.Features.EmotionTags.Queries.GetAllEmotionTags;

public record GetAllEmotionTagsQuery(
    Guid? AliasId = null,
    bool? IsActive = null
) : IQuery<GetAllEmotionTagsResult>;

public record GetAllEmotionTagsResult(
    IEnumerable<EmotionTagDto> EmotionTags
);
