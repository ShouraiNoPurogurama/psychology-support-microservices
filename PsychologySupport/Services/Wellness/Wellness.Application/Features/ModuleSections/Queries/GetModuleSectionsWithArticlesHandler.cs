using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.Queries.Media;
using BuildingBlocks.Pagination;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Features.ModuleSections.Dtos;
using Wellness.Domain.Enums;
using Wellness.Application.Exceptions;

public record GetModuleSectionsWithArticlesQuery(
    Guid ModuleId,
    Guid SubjectRef,
    int PageIndex,
    int PageSize
) : IQuery<GetModuleSectionsWithArticlesResult>;

public record GetModuleSectionsWithArticlesResult(PaginatedResult<ModuleSectionDetailsDto> Sections);

public class GetModuleSectionsWithArticlesHandler
    : IQueryHandler<GetModuleSectionsWithArticlesQuery, GetModuleSectionsWithArticlesResult>
{
    private readonly IWellnessDbContext _context;
    private readonly IRequestClient<GetMediaUrlRequest> _getMediaUrlClient;

    public GetModuleSectionsWithArticlesHandler(
        IWellnessDbContext context,
        IRequestClient<GetMediaUrlRequest> getMediaUrlClient)
    {
        _context = context;
        _getMediaUrlClient = getMediaUrlClient;
    }

    public async Task<GetModuleSectionsWithArticlesResult> Handle(GetModuleSectionsWithArticlesQuery request, CancellationToken cancellationToken)
    {
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

        // Áp dụng pagination trên SectionArticles
        var pagedArticles = allArticles
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Lấy tất cả MediaIds cần fetch URL
        var mediaIds = pagedArticles
            .Select(a => a.Article.MediaId)
            .Concat(pagedArticles.Select(a => a.ModuleSection.MediaId))
            .Distinct()
            .ToList();

        var mediaResponse = await _getMediaUrlClient
            .GetResponse<GetMediaUrlResponse>(new GetMediaUrlRequest { MediaIds = mediaIds }, cancellationToken);
        var mediaUrls = mediaResponse.Message.Urls;

        // Map thành ModuleSectionDetailsDto
        var groupedBySection = pagedArticles.GroupBy(a => a.ModuleSection.Id);
        var dtoList = groupedBySection.Select(g =>
        {
            var ms = g.First().ModuleSection;

            // Progress của module section
            var sectionProgress = ms.ModuleProgresses
                .FirstOrDefault(p => p.SubjectRef == request.SubjectRef && p.SectionId == ms.Id);

            int completedDuration = sectionProgress?.MinutesRead ?? 0;
            bool sectionCompleted = sectionProgress?.ProcessStatus == ProcessStatus.Completed;

            var articles = g.Select(a =>
            {
                var progress = a.Article.ArticleProgresses
                    .FirstOrDefault(p => p.ModuleProgress != null && p.ModuleProgress.SubjectRef == request.SubjectRef);

                bool completed = progress?.ProcessStatus == ProcessStatus.Completed;

                return new SectionArticleDto(
                    a.Article.Id,
                    a.Article.Title,
                    mediaUrls.TryGetValue(a.Article.MediaId, out var url) ? url : string.Empty,
                    a.Article.ContentJson,
                    a.Article.OrderIndex,
                    a.Article.Duration,
                    completed,
                    a.Article.Source
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

        var paginatedResult = new PaginatedResult<ModuleSectionDetailsDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            dtoList
        );

        return new GetModuleSectionsWithArticlesResult(paginatedResult);
    }

}
