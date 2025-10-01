using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.Queries.Media;
using BuildingBlocks.Pagination;
using BuildingBlocks.Utils;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Translation.API.Protos;
using Wellness.Application.Data;
using Wellness.Application.Exceptions;
using Wellness.Application.Features.ModuleSections.Dtos;
using Wellness.Domain.Aggregates.ModuleSections;
using Wellness.Domain.Enums;

public record GetModuleSectionsQuery(
    Guid WellnessModuleId,
    Guid SubjectRef,
    PaginationRequest PaginationRequest,
    string? TargetLang = null
) : IQuery<GetModuleSectionsResult>;

public record GetModuleSectionsResult(PaginatedResult<ModuleSectionDto> Sections);

public class GetModuleSectionsHandler : IQueryHandler<GetModuleSectionsQuery, GetModuleSectionsResult>
{
    private readonly IWellnessDbContext _context;
    private readonly IRequestClient<GetMediaUrlRequest> _getMediaUrlClient;
    private readonly TranslationService.TranslationServiceClient _translationClient;

    public GetModuleSectionsHandler(
        IWellnessDbContext context,
        IRequestClient<GetMediaUrlRequest> getMediaUrlClient,
        TranslationService.TranslationServiceClient translationClient)
    {
        _context = context;
        _getMediaUrlClient = getMediaUrlClient;
        _translationClient = translationClient;
    }

    public async Task<GetModuleSectionsResult> Handle(GetModuleSectionsQuery request, CancellationToken cancellationToken)
    {
        int skip = (request.PaginationRequest.PageIndex - 1) * request.PaginationRequest.PageSize;
        int take = request.PaginationRequest.PageSize;

        // 1️⃣ Lấy module sections chính
        var sections = await _context.ModuleSections
            .AsNoTracking()
            .Where(ms => ms.ModuleId == request.WellnessModuleId)
            .OrderBy(ms => ms.Title)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        if (!sections.Any())
            throw new WellnessNotFoundException($"Không tìm thấy mục học phần nào cho WellnessModuleId '{request.WellnessModuleId}'.");

        var sectionIds = sections.Select(s => s.Id).ToList();

        // 2️⃣ Lấy count bài viết cho tất cả sectionIds
        var articleCounts = await _context.SectionArticles
            .AsNoTracking()
            .Where(sa => sa.SectionId.HasValue && sectionIds.Contains(sa.SectionId.Value))
            .GroupBy(sa => sa.SectionId)
            .Select(g => new { SectionId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var articleCountDict = articleCounts.ToDictionary(a => a.SectionId, a => a.Count);

        // 3️⃣ Lấy progress cho tất cả sectionIds với SubjectRef
        var progresses = await _context.ModuleProgresses
            .AsNoTracking()
            .Where(mp => sectionIds.Contains(mp.SectionId) && mp.SubjectRef == request.SubjectRef)
            .GroupBy(mp => mp.SectionId)
            .Select(g => g.OrderBy(mp => mp.Id).FirstOrDefault())
            .ToListAsync(cancellationToken);
        var progressDict = progresses.Where(p => p != null).ToDictionary(p => p!.SectionId, p => p!);

        // 4️⃣ Lấy Media URLs
        var mediaIds = sections.Select(s => s.MediaId).Distinct().ToList();
        var mediaResponse = await _getMediaUrlClient
            .GetResponse<GetMediaUrlResponse>(new GetMediaUrlRequest { MediaIds = mediaIds }, cancellationToken);
        var mediaUrls = mediaResponse.Message.Urls;

        // 5️⃣ Translation batch
        Dictionary<string, string>? translations = null;
        if (!string.IsNullOrEmpty(request.TargetLang))
        {
            var translationDict = TranslationUtils.CreateBuilder()
                .AddEntities(sections, nameof(ModuleSection), s => s.Title)
                .AddEntities(sections, nameof(ModuleSection), s => s.Description)
                .Build();

            var translationResponse = await _translationClient.TranslateDataAsync(
                new TranslateDataRequest
                {
                    Originals = { translationDict },
                    TargetLanguage = request.TargetLang
                },
                cancellationToken: cancellationToken
            );

            translations = translationResponse.Translations.Any()
                ? translationResponse.Translations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : new Dictionary<string, string>();
        }

        // 6️⃣ Map sang DTO sử dụng object đã translate
        var sectionDtos = sections.Select(ms =>
        {
            var translated = translations?.MapTranslatedProperties(
                ms,
                nameof(ModuleSection),
                ms.Id.ToString(),
                x => x.Title,
                x => x.Description
            );

            return new ModuleSectionDto(
                ms.Id,
                translated?.Title ?? ms.Title,
                mediaUrls.TryGetValue(ms.MediaId, out var url) ? url : string.Empty,
                translated?.Description ?? ms.Description,
                ms.TotalDuration,
                progressDict.GetValueOrDefault(ms.Id)?.MinutesRead ?? 0,
                progressDict.GetValueOrDefault(ms.Id)?.ProcessStatus ?? ProcessStatus.NotStarted,
                articleCountDict.GetValueOrDefault(ms.Id, 0)
            );
        }).ToList();

        // 7️⃣ Tạo kết quả phân trang
        var totalCount = await _context.ModuleSections
            .AsNoTracking()
            .CountAsync(ms => ms.ModuleId == request.WellnessModuleId, cancellationToken);

        var paginatedResult = new PaginatedResult<ModuleSectionDto>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            sectionDtos
        );

        return new GetModuleSectionsResult(paginatedResult);
    }
}
