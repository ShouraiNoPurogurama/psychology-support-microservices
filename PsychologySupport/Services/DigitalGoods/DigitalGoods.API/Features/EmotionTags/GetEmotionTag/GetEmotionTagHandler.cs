using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.Queries.Media;
using DigitalGoods.API.Data;
using DigitalGoods.API.Dtos;
using DigitalGoods.API.Enums;
using DigitalGoods.API.Models;
using Microsoft.EntityFrameworkCore;
using Translation.API.Protos;
using MassTransit;
using DigitalGoods.API.Extensions;

namespace DigitalGoods.API.Features.EmotionTags.GetEmotionTag;

public record GetEmotionTagsByTopicQuery(Guid SubjectRef) : IQuery<GetEmotionTagsByTopicResult>;

public record GetEmotionTagsByTopicResult(
    Dictionary<string, List<EmotionTagDto>> GroupedByTopic
);

public class GetEmotionTagHandler
    : IQueryHandler<GetEmotionTagsByTopicQuery, GetEmotionTagsByTopicResult>
{
    private readonly DigitalGoodsDbContext _dbContext;
    private readonly TranslationService.TranslationServiceClient _translationClient;
    private readonly IRequestClient<GetMediaUrlRequest> _getMediaUrlClient;

    public GetEmotionTagHandler(
        DigitalGoodsDbContext dbContext,
        TranslationService.TranslationServiceClient translationClient,
        IRequestClient<GetMediaUrlRequest> getMediaUrlClient
    )
    {
        _dbContext = dbContext;
        _translationClient = translationClient;
        _getMediaUrlClient = getMediaUrlClient;
    }

    public async Task<GetEmotionTagsByTopicResult> Handle(
        GetEmotionTagsByTopicQuery request,
        CancellationToken cancellationToken)
    {
        var tags = await _dbContext.EmotionTags
            .AsNoTracking()
            .Include(t => t.DigitalGoods)
            .OrderBy(t => t.SortOrder)
            .ToListAsync(cancellationToken);

        if (!tags.Any())
            return new GetEmotionTagsByTopicResult(new Dictionary<string, List<EmotionTagDto>>());

        //  Resolve MediaUrl từ MediaId
        var mediaIds = tags
            .Where(t => t.MediaId.HasValue)
            .Select(t => t.MediaId!.Value)
            .Distinct()
            .ToList();

        var mediaUrls = new Dictionary<Guid, string>();
        if (mediaIds.Any())
        {
            var mediaResponse = await _getMediaUrlClient.GetResponse<GetMediaUrlResponse>(
                new GetMediaUrlRequest { MediaIds = mediaIds },
                cancellationToken
            );
            mediaUrls = mediaResponse.Message.Urls;
        }

        // Lấy danh sách DigitalGood mà user đang sở hữu
        var ownedDigitalGoodIds = await _dbContext.Inventories
            .AsNoTracking()
            .Where(i => i.Subject_ref == request.SubjectRef)
            .Select(i => i.DigitalGoodId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Xác định IsOwnedByUser + MediaUrl
        var tagDtos = tags.Select(t =>
        {
            bool isOwned = t.Scope == EmotionTagScope.Free
                ? true
                : t.DigitalGoods.Any(d => ownedDigitalGoodIds.Contains(d.Id));

            string? mediaUrl = null;
            if (t.MediaId.HasValue && mediaUrls.TryGetValue(t.MediaId.Value, out var url))
                mediaUrl = url;

            return new EmotionTagDto(
                t.Id,
                t.Code,
                t.DisplayName,
                t.UnicodeCodepoint,
                t.Topic,
                t.SortOrder,
                t.IsActive,
                t.Scope.ToString(),
                mediaUrl,
                isOwned
            );
        }).ToList();

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
