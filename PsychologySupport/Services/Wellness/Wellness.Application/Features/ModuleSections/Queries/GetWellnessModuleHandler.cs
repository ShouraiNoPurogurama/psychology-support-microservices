using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Wellness.Application.Data;
using Wellness.Application.Exceptions;
using Wellness.Application.Features.ModuleSections.Dtos;
using Wellness.Domain.Aggregates.ModuleSections;
using Translation.API.Protos;
using BuildingBlocks.Messaging.Events.Queries.Media;
using Wellness.Application.Extensions;

public record GetWellnessModulesQuery(PaginationRequest PaginationRequest, string? TargetLang = null)
    : IQuery<GetWellnessModulesResult>;

public record GetWellnessModulesResult(PaginatedResult<WellnessModuleDto> Modules);

public class GetWellnessModulesHandler : IQueryHandler<GetWellnessModulesQuery, GetWellnessModulesResult>
{
    private readonly IWellnessDbContext _context;
    private readonly IRequestClient<GetMediaUrlRequest> _getMediaUrlClient;
    private readonly TranslationService.TranslationServiceClient _translationClient;

    public GetWellnessModulesHandler(
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

        if (!rawModules.Any())
        {
            var empty = new PaginatedResult<WellnessModuleDto>(
                request.PaginationRequest.PageIndex,
                request.PaginationRequest.PageSize,
                totalCount,
                new List<WellnessModuleDto>()
            );
            return new GetWellnessModulesResult(empty);
        }

        // Resolve MediaUrl
        var mediaIds = rawModules.Select(m => m.MediaId).Distinct().ToList();
        var mediaResponse = await _getMediaUrlClient
            .GetResponse<GetMediaUrlResponse>(new GetMediaUrlRequest { MediaIds = mediaIds }, cancellationToken);
        var mediaUrls = mediaResponse.Message.Urls;

        // Translation
        if (!string.IsNullOrEmpty(request.TargetLang) && request.TargetLang == "vi")
        {
            try
            {
                rawModules = await rawModules.TranslateEntitiesAsync(
                    nameof(WellnessModule),
                    _translationClient,
                    m => m.Id.ToString(),
                    cancellationToken,
                    m => m.Name,
                    m => m.Description
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationError] {ex.Message}");
            }
        }

        // Map to DTO with translations 
        var moduleDtos = rawModules.Select(m =>
        {
            return new WellnessModuleDto(
                m.Id,
                m.Name,
                mediaUrls.TryGetValue(m.MediaId, out var url) ? url : string.Empty,
                m.Description,
                m.ModuleSections.Count
            );
        }).ToList();


        var paginatedResult = new PaginatedResult<WellnessModuleDto>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            moduleDtos
        );

        return new GetWellnessModulesResult(paginatedResult);
    }
}