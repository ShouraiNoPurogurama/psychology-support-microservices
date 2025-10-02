using BuildingBlocks.CQRS;
using DigitalGoods.API.Data;
using DigitalGoods.API.Dtos;
using DigitalGoods.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalGoods.API.Features.EmotionTags.GetEmotionTag;

public record GetEmotionTagsByTopicQuery() : IQuery<GetEmotionTagsByTopicResult>;

public record GetEmotionTagsByTopicResult(
    Dictionary<string, List<EmotionTagDto>> GroupedByTopic
);

public class GetEmotionTagHandler
    : IQueryHandler<GetEmotionTagsByTopicQuery, GetEmotionTagsByTopicResult>
{
    private readonly DigitalGoodsDbContext _dbContext;

    public GetEmotionTagHandler(DigitalGoodsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetEmotionTagsByTopicResult> Handle(
        GetEmotionTagsByTopicQuery request,
        CancellationToken cancellationToken)
    {
        var tags = await _dbContext.EmotionTags
            .AsNoTracking()
            .OrderBy(t => t.SortOrder)
            .ToListAsync(cancellationToken);

        var grouped = tags
            .GroupBy(t => t.Topic ?? "Uncategorized")
            .ToDictionary(
                g => g.Key,
                g => g.Select(t => new EmotionTagDto(
                    t.Id,
                    t.Code,
                    t.DisplayName,
                    t.UnicodeCodepoint,
                    t.Topic,
                    t.SortOrder,
                    t.IsActive,
                    t.Scope.ToString(),
                    t.MediaId
                )).ToList()
            );

        return new GetEmotionTagsByTopicResult(grouped);
    }
}
