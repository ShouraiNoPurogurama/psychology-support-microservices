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
        var query = _context.ModuleSections
            .AsNoTracking()
            .Include(ms => ms.SectionArticles)
                .ThenInclude(a => a.ArticleProgresses)
            .Include(ms => ms.ModuleProgresses)
            .Where(ms => ms.ModuleId == request.ModuleId);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
            throw new WellnessNotFoundException($"Không tìm thấy module sections cho ModuleId '{request.ModuleId}'.");

        var sections = await query
            .OrderBy(ms => ms.Title)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Lấy tất cả MediaId (ModuleSection + SectionArticles)
        var mediaIds = sections.Select(ms => ms.MediaId)
            .Concat(sections.SelectMany(ms => ms.SectionArticles.Select(a => a.MediaId)))
            .Distinct()
            .ToList();

        var mediaResponse = await _getMediaUrlClient
            .GetResponse<GetMediaUrlResponse>(new GetMediaUrlRequest { MediaIds = mediaIds }, cancellationToken);
        var mediaUrls = mediaResponse.Message.Urls;

        var dtoList = sections.Select(ms =>
        {
            // Progress của module section
            var sectionProgress = ms.ModuleProgresses
                .FirstOrDefault(p => p.SubjectRef == request.SubjectRef && p.SectionId == ms.Id);

            int completedDuration = sectionProgress?.MinutesRead ?? 0;
            bool sectionCompleted = sectionProgress?.ProcessStatus == ProcessStatus.Completed;

            // Danh sách bài viết
            var articles = ms.SectionArticles
                .OrderBy(a => a.OrderIndex)
                .Select(a =>
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

        var paginatedResult = new PaginatedResult<ModuleSectionDetailsDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            dtoList
        );

        return new GetModuleSectionsWithArticlesResult(paginatedResult);
    }
}
