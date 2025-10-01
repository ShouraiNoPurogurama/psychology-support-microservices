using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.Queries.Media;
using BuildingBlocks.Pagination;
using BuildingBlocks.Utils;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Translation.API.Protos;
using Wellness.Application.Data;
using Wellness.Application.Exceptions;
using Wellness.Application.Features.ModuleSections.Dtos;
using Wellness.Domain.Aggregates.ModuleSections;
using Wellness.Domain.Enums;

public record GetModuleSectionsWithArticlesQuery(
    Guid ModuleId,
    Guid SubjectRef,
    PaginationRequest PaginationRequest,
    string? TargetLang = null
) : IQuery<GetModuleSectionsWithArticlesResult>;

public record GetModuleSectionsWithArticlesResult(PaginatedResult<ModuleSectionDetailsDto> Sections);


public class GetModuleSectionsWithArticlesHandler
    : IQueryHandler<GetModuleSectionsWithArticlesQuery, GetModuleSectionsWithArticlesResult>
{
    private readonly IWellnessDbContext _context;
    private readonly IRequestClient<GetMediaUrlRequest> _getMediaUrlClient;
    private readonly TranslationService.TranslationServiceClient _translationClient;

    public GetModuleSectionsWithArticlesHandler(
        IWellnessDbContext context,
        IRequestClient<GetMediaUrlRequest> getMediaUrlClient,
        TranslationService.TranslationServiceClient translationClient)
    {
        _context = context;
        _getMediaUrlClient = getMediaUrlClient;
        _translationClient = translationClient;
    }

    public async Task<GetModuleSectionsWithArticlesResult> Handle(GetModuleSectionsWithArticlesQuery request, CancellationToken cancellationToken)
    {
        int skip = (request.PaginationRequest.PageIndex - 1) * request.PaginationRequest.PageSize;
        int take = request.PaginationRequest.PageSize;

        var sections = await _context.ModuleSections
            .AsNoTracking()
            .Include(ms => ms.SectionArticles)
                .ThenInclude(a => a.ArticleProgresses)
                    .ThenInclude(ap => ap.ModuleProgress)
            .Include(ms => ms.ModuleProgresses)
            .Where(ms => ms.Id == request.ModuleId)
            .OrderBy(ms => ms.Title)
            .ToListAsync(cancellationToken);

        if (!sections.Any())
            throw new WellnessNotFoundException($"Không tìm thấy module sections cho ModuleId '{request.ModuleId}'.");

        // Flatten tất cả SectionArticles
        var allArticles = sections
            .SelectMany(ms => ms.SectionArticles.Select(a => new { ModuleSection = ms, Article = a }))
            .OrderBy(a => a.Article.OrderIndex)
            .ToList();

        var totalCount = allArticles.Count;

        // Áp dụng pagination
        var pagedArticles = allArticles.Skip(skip).Take(take).ToList();

        // Lấy tất cả MediaIds
        var mediaIds = pagedArticles
            .Select(a => a.Article.MediaId)
            .Concat(pagedArticles.Select(a => a.ModuleSection.MediaId))
            .Distinct()
            .ToList();

        var mediaResponse = await _getMediaUrlClient
            .GetResponse<GetMediaUrlResponse>(new GetMediaUrlRequest { MediaIds = mediaIds }, cancellationToken);
        var mediaUrls = mediaResponse.Message.Urls;

        // Translation
        Dictionary<string, string>? translations = null;
        if (!string.IsNullOrEmpty(request.TargetLang))
        {
            var translationDict = TranslationUtils.CreateBuilder()
                .AddEntities(sections, nameof(ModuleSection), ms => ms.Title)
                .AddEntities(sections, nameof(ModuleSection), ms => ms.Description)
                .AddEntities(pagedArticles.Select(a => a.Article), nameof(SectionArticle), a => a.Title)
                .Build();

            var translationRequest = new TranslateDataRequest
            {
                Originals = { translationDict },
                TargetLanguage = request.TargetLang
            };

            var translationResponse = await _translationClient
                .TranslateDataAsync(translationRequest, cancellationToken: cancellationToken);

            translations = translationResponse.Translations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        // Map thành DTO
        var groupedBySection = pagedArticles.GroupBy(a => a.ModuleSection.Id);
        var dtoList = groupedBySection.Select(g =>
        {
            var ms = g.First().ModuleSection;

            // Dùng MapTranslatedProperties cho ModuleSection
            var translatedSection = translations!.MapTranslatedProperties(
                ms,
                nameof(ModuleSection),
                ms.Id.ToString(),
                x => x.Title,
                x => x.Description
            );

            var sectionProgress = ms.ModuleProgresses
                .FirstOrDefault(p => p.SubjectRef == request.SubjectRef && p.SectionId == ms.Id);

            int completedDuration = sectionProgress?.MinutesRead ?? 0;
            bool sectionCompleted = sectionProgress?.ProcessStatus == ProcessStatus.Completed;

            // Dùng MapTranslatedPropertiesForCollection cho Article
            var translatedArticles = translations!.MapTranslatedPropertiesForCollection(
                g.Select(x => x.Article),
                nameof(SectionArticle),
                a => a.Title
            );

            var articles = translatedArticles.Select(a =>
            {
                var progress = a.ArticleProgresses
                    .FirstOrDefault(p => p.ModuleProgress != null && p.ModuleProgress.SubjectRef == request.SubjectRef);
                bool completed = progress?.ProcessStatus == ProcessStatus.Completed;

                return new SectionArticleDto(
                    a.Id,
                    a.Title, 
                    mediaUrls.TryGetValue(a.MediaId, out var url) ? url : string.Empty,
                    a.ContentJson,
                    a.OrderIndex,
                    a.Duration,
                    completed,
                    a.Source
                );
            }).ToList();

            return new ModuleSectionDetailsDto(
                ms.Id,
                translatedSection.Title,
                mediaUrls.TryGetValue(ms.MediaId, out var sectionUrl) ? sectionUrl : string.Empty,
                translatedSection.Description,
                ms.TotalDuration,
                completedDuration,
                sectionCompleted,
                ms.SectionArticles.Count,
                articles
            );
        }).ToList();


        var paginatedResult = new PaginatedResult<ModuleSectionDetailsDto>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            dtoList
        );

        return new GetModuleSectionsWithArticlesResult(paginatedResult);
    }
}
