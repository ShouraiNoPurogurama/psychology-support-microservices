using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.Queries.Media;
using BuildingBlocks.Pagination;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Translation.API.Protos;
using Wellness.Application.Data;
using Wellness.Application.Exceptions;
using Wellness.Application.Extensions;
using Wellness.Application.Features.ModuleSections.Dtos;
using Wellness.Domain.Aggregates.ModuleSections;
using Wellness.Domain.Enums;

using Wellness.Domain.Aggregates.ModuleSections.Enums;

public record GetModuleSectionsWithArticlesQuery(
    Guid? ModuleSectionId,
    Guid SubjectRef,
    PaginationRequest PaginationRequest,
    string? TargetLang = null,
    ModuleCategory? Category = null
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

        var sectionsQuery = _context.ModuleSections
             .AsNoTracking()
             .Include(ms => ms.SectionArticles)
                 .ThenInclude(a => a.ArticleProgresses)
                     .ThenInclude(ap => ap.ModuleProgress)
             .Include(ms => ms.ModuleProgresses)
             .Where(ms =>
                 (!request.ModuleSectionId.HasValue || ms.Id == request.ModuleSectionId) &&
                 (!request.Category.HasValue || ms.Category == request.Category.Value)
             )
             .OrderBy(ms => ms.Title);


        var totalSectionsCount = await sectionsQuery.CountAsync(cancellationToken);

        if (totalSectionsCount == 0)
            throw new WellnessNotFoundException($"Không tìm thấy module sections cho ModuleId '{request.ModuleSectionId}'.");

        var sections = await sectionsQuery.ToListAsync(cancellationToken);

        // Flatten tất cả SectionArticles
        var allArticles = sections
            .SelectMany(ms => ms.SectionArticles.Select(a => new { ModuleSection = ms, Article = a }))
            .OrderBy(a => a.Article.OrderIndex)
            .ToList();

        var totalCount = allArticles.Count;

        // Áp dụng pagination
        var pagedArticles = allArticles.Skip(skip).Take(take).ToList();

        // Lấy tất cả MediaIds
        var allArticlesForMedia = sections.SelectMany(ms => ms.SectionArticles).ToList();
        var mediaIds = sections.Select(ms => ms.MediaId)
            .Concat(allArticlesForMedia.Select(a => a.MediaId))
            .Distinct()
            .ToList();

        var mediaResponse = await _getMediaUrlClient
            .GetResponse<GetMediaUrlResponse>(new GetMediaUrlRequest { MediaIds = mediaIds }, cancellationToken);
        var mediaUrls = mediaResponse.Message.Urls;

        // Translation
        if (!string.IsNullOrEmpty(request.TargetLang) && request.TargetLang == "vi")
        {
            try
            {
                // Translate sections
                await sections.TranslateEntitiesAsync(
                    nameof(ModuleSection),
                    _translationClient,
                    ms => ms.Id.ToString(),
                    cancellationToken,
                    ms => ms.Title,
                    ms => ms.Description
                );

                // Translate all articles
                if (allArticlesForMedia.Any())
                {
                    await allArticlesForMedia.TranslateEntitiesAsync(
                        nameof(SectionArticle),
                        _translationClient,
                        a => a.Id.ToString(),
                        cancellationToken,
                        a => a.Title
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationError] {ex.Message}");
            }
        }

        // Map thành DTO
        var groupedBySection = pagedArticles.GroupBy(a => a.ModuleSection.Id);
        var dtoList = groupedBySection.Select(g =>
        {
            var ms = g.First().ModuleSection;

            var sectionProgress = ms.ModuleProgresses
                .FirstOrDefault(p => p.SubjectRef == request.SubjectRef && p.SectionId == ms.Id);

            int completedDuration = sectionProgress?.MinutesRead ?? 0;
            bool sectionCompleted = sectionProgress?.ProcessStatus == ProcessStatus.Completed;

            var articles = g.Select(x => x.Article).Select(a =>
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
                ms.Title,
                mediaUrls.TryGetValue(ms.MediaId, out var sectionUrl) ? sectionUrl : string.Empty,
                ms.Description,
                ms.TotalDuration,
                completedDuration,
                sectionCompleted,
                ms.SectionArticles.Count,
                articles
            );
        }).ToList();

        if (!pagedArticles.Any())
        {
            var empty = new PaginatedResult<ModuleSectionDetailsDto>(
                request.PaginationRequest.PageIndex,
                request.PaginationRequest.PageSize,
                totalCount,
                new List<ModuleSectionDetailsDto>()
            );
            return new GetModuleSectionsWithArticlesResult(empty);
        }

        var paginatedResult = new PaginatedResult<ModuleSectionDetailsDto>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            dtoList
        );

        return new GetModuleSectionsWithArticlesResult(paginatedResult);
    }
}