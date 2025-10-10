using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Queries.Translation;
using BuildingBlocks.Utils;
using DigitalGoods.API.Data;
using DigitalGoods.API.Dtos;
using DigitalGoods.API.Models;
using DigitalGoods.API.Extensions;
using Microsoft.EntityFrameworkCore;
using Translation.API.Protos;

namespace DigitalGoods.API.Features.EmotionTags.GetEmotionTag;

public record GetEmotionTagsByTopicQuery() : IQuery<GetEmotionTagsByTopicResult>;

public record GetEmotionTagsByTopicResult(
    Dictionary<string, List<EmotionTagDto>> GroupedByTopic
);

public class GetEmotionTagHandler
    : IQueryHandler<GetEmotionTagsByTopicQuery, GetEmotionTagsByTopicResult>
{
    private readonly DigitalGoodsDbContext _dbContext;
    private readonly TranslationService.TranslationServiceClient _translationClient;

    public GetEmotionTagHandler(
        DigitalGoodsDbContext dbContext,
        TranslationService.TranslationServiceClient translationClient
    )
    {
        _dbContext = dbContext;
        _translationClient = translationClient;
    }

    public async Task<GetEmotionTagsByTopicResult> Handle(
        GetEmotionTagsByTopicQuery request,
        CancellationToken cancellationToken)
    {
        var tags = await _dbContext.EmotionTags
            .AsNoTracking()
            .OrderBy(t => t.SortOrder)
            .ToListAsync(cancellationToken);

        var tagDtos = tags.Select(t => new EmotionTagDto(
            t.Id,
            t.Code,
            t.DisplayName,
            t.UnicodeCodepoint,
            t.Topic,
            t.SortOrder,
            t.IsActive,
            t.Scope.ToString(),
            t.MediaId
        )).ToList();

        var translatedTags = await tagDtos.TranslateEntitiesAsync(
            nameof(EmotionTag),
            _translationClient,
            t => t.Id.ToString(),
            cancellationToken,
            t => t.DisplayName,
            t => t.Topic
        );

        var grouped = translatedTags
            .GroupBy(t => t.Topic ?? "Uncategorized")
            .ToDictionary(
                g => g.Key,
                g => g.ToList()
            );

        return new GetEmotionTagsByTopicResult(grouped);
    }
}
