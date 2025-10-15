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

namespace DigitalGoods.API.Features.DigitalGoods.GetDigitalGood;

public record GetDigitalGoodsQuery(Guid SubjectRef, DigitalGoodType? TypeFilter = null) : IQuery<GetDigitalGoodsResult>;

public record GetDigitalGoodsResult(List<DigitalGoodDto> DigitalGoods);

public class GetDigitalGoodHandler : IQueryHandler<GetDigitalGoodsQuery, GetDigitalGoodsResult>
{
    private readonly DigitalGoodsDbContext _dbContext;
    private readonly TranslationService.TranslationServiceClient _translationClient;
    private readonly IRequestClient<GetMediaUrlRequest> _getMediaUrlClient;

    public GetDigitalGoodHandler(
        DigitalGoodsDbContext dbContext,
        TranslationService.TranslationServiceClient translationClient,
        IRequestClient<GetMediaUrlRequest> getMediaUrlClient)
    {
        _dbContext = dbContext;
        _translationClient = translationClient;
        _getMediaUrlClient = getMediaUrlClient;
    }

    public async Task<GetDigitalGoodsResult> Handle(GetDigitalGoodsQuery request, CancellationToken cancellationToken)
    {
        var goods = await _dbContext.DigitalGoods
            .AsNoTracking()
            .Where(d => d.IsActive)
            .Where(d => !request.TypeFilter.HasValue || d.Type == request.TypeFilter.Value)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        if (!goods.Any())
            return new GetDigitalGoodsResult(new List<DigitalGoodDto>());

        // Resolve MediaUrl from MediaId
        var mediaIds = goods
            .Where(g => g.MediaId.HasValue)
            .Select(g => g.MediaId!.Value)
            .Distinct()
            .ToList();

        var mediaUrls = new Dictionary<Guid, string>();
        if (mediaIds.Any())
        {
            var mediaResponse = await _getMediaUrlClient.GetResponse<GetMediaUrlResponse>(
                new GetMediaUrlRequest { MediaIds = mediaIds },
                cancellationToken);
            mediaUrls = mediaResponse.Message.Urls;
        }

        // Get owned DigitalGood IDs where inventory is active
        var ownedDigitalGoodIds = await _dbContext.Inventories
            .AsNoTracking()
            .Where(i => i.Subject_ref == request.SubjectRef && i.Status == InventoryStatus.Active)
            .Select(i => i.DigitalGoodId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var dtos = goods.Select(g =>
        {
            bool isOwned = ownedDigitalGoodIds.Contains(g.Id);
            string? mediaUrl = null;
            if (g.MediaId.HasValue && mediaUrls.TryGetValue(g.MediaId.Value, out var url))
                mediaUrl = url;

            return new DigitalGoodDto(
                g.Id,
                g.Name,
                g.Type.ToString(),
                g.ConsumptionType.ToString(),
                g.Price,
                g.Description,
                mediaUrl,
                isOwned);
        }).ToList();

        // Translate Name only
        var translatedDtos = await dtos.TranslateEntitiesAsync(
            nameof(DigitalGood),
            _translationClient,
            d => d.Id.ToString(),
            cancellationToken,
            d => d.Name);

        return new GetDigitalGoodsResult(translatedDtos);
    }
}