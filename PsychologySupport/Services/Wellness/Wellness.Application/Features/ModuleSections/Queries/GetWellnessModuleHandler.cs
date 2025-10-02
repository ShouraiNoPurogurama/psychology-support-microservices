using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using BuildingBlocks.Utils;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Wellness.Application.Data;
using Wellness.Application.Exceptions;
using Wellness.Application.Features.ModuleSections.Dtos;
using Wellness.Domain.Aggregates.ModuleSections;
using Translation.API.Protos;
using BuildingBlocks.Messaging.Events.Queries.Media;

public record GetWellnessModulesQuery(PaginationRequest PaginationRequest, string? TargetLang = null)
    : IQuery<GetWellnessModulesResult>;

public record GetWellnessModulesResult(PaginatedResult<WellnessModuleDto> Modules);

public class GetWellnessModuleHandler : IQueryHandler<GetWellnessModulesQuery, GetWellnessModulesResult>
{
    private readonly IWellnessDbContext _context;
    private readonly IRequestClient<GetMediaUrlRequest> _getMediaUrlClient;
    private readonly TranslationService.TranslationServiceClient _translationClient;

    public GetWellnessModuleHandler(
        IWellnessDbContext context,
        IRequestClient<GetMediaUrlRequest> getMediaUrlClient,
        TranslationService.TranslationServiceClient translationClient)
    {
        _context = context;
        _getMediaUrlClient = getMediaUrlClient;
        _translationClient = translationClient;
    }

    public async Task<GetWellnessModulesResult> Handle(GetWellnessModulesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WellnessModules.AsNoTracking()
            .Include(m => m.ModuleSections)
            .OrderBy(m => m.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
            throw new WellnessNotFoundException("Không tìm thấy module nào.");

        var rawModules = await query
            .Skip((request.PaginationRequest.PageIndex - 1) * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .ToListAsync(cancellationToken);

        // Resolve MediaUrl
        var mediaIds = rawModules.Select(m => m.MediaId).Distinct().ToList();
        var mediaResponse = await _getMediaUrlClient
            .GetResponse<GetMediaUrlResponse>(new GetMediaUrlRequest { MediaIds = mediaIds }, cancellationToken);
        var mediaUrls = mediaResponse.Message.Urls;

        // Build translation dictionary
        var translationDict = TranslationUtils.CreateBuilder()
            .AddEntities(rawModules, nameof(WellnessModule), m => m.Name)
            .AddEntities(rawModules, nameof(WellnessModule), m => m.Description)
            .Build();

        Dictionary<string, string>? translations = null;
        if (!string.IsNullOrEmpty(request.TargetLang))
        {
            var translationRequest = new TranslateDataRequest
            {
                Originals = { translationDict },
                TargetLanguage = request.TargetLang ?? SupportedLang.vi.ToString()
            };

            var translationResponse = await _translationClient
                .TranslateDataAsync(translationRequest, cancellationToken: cancellationToken);

            translations = translationResponse.Translations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        // Map to DTO with translations 
        var translatedModules = rawModules.Select(m =>
        {
            var translated = translations?.MapTranslatedProperties(
                m,
                nameof(WellnessModule),
                id: m.Id.ToString(),
                x => x.Name,
                x => x.Description
            ) ?? m;

            return new WellnessModuleDto(
                m.Id,
                translated.Name,
                mediaUrls.TryGetValue(m.MediaId, out var url) ? url : string.Empty,
                translated.Description,
                m.ModuleSections.Count
            );
        }).ToList();

        
        var paginatedResult = new PaginatedResult<WellnessModuleDto>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            translatedModules
        );

        return new GetWellnessModulesResult(paginatedResult);
    }
}
