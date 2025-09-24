using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Queries.Media;
using BuildingBlocks.Pagination;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Exceptions;
using Wellness.Application.Features.ModuleSections.Dtos;
using Wellness.Domain.Enums;

public record GetModuleSectionsQuery(Guid WellnessModuleId, Guid SubjectRef,int PageIndex, int PageSize)
    : IQuery<GetModuleSectionsResult>;

public record GetModuleSectionsResult(PaginatedResult<ModuleSectionDto> Sections);

public class GetModuleSectionsHandler
    : IQueryHandler<GetModuleSectionsQuery, GetModuleSectionsResult>
{
    private readonly IWellnessDbContext _context;
    private readonly IRequestClient<GetMediaUrlRequest> _getMediaUrlClient;

    public GetModuleSectionsHandler(
        IWellnessDbContext context,
        IRequestClient<GetMediaUrlRequest> getMediaUrlClient)
    {
        _context = context;
        _getMediaUrlClient = getMediaUrlClient;
    }

    public async Task<GetModuleSectionsResult> Handle(GetModuleSectionsQuery request, CancellationToken cancellationToken)
    {
        // Lọc theo ModuleId
        var query = _context.ModuleSections
            .AsNoTracking()
            .Include(ms => ms.SectionArticles)
            .Include(ms => ms.ModuleProgresses)
            .Where(ms => ms.ModuleId == request.WellnessModuleId);

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
            throw new WellnessNotFoundException($"Không tìm thấy mục học phần nào cho WellnessModuleId '{request.WellnessModuleId}'.");

        var sections = await query
            .OrderBy(ms => ms.Title)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Lấy tất cả MediaId để gửi batch request
        var mediaIds = sections.Select(ms => ms.MediaId).Distinct().ToList();

        var mediaResponse = await _getMediaUrlClient
            .GetResponse<GetMediaUrlResponse>(new GetMediaUrlRequest { MediaIds = mediaIds }, cancellationToken);

        var mediaUrls = mediaResponse.Message.Urls;

        var dtoList = sections.Select(ms =>
        {
            var progress = ms.ModuleProgresses
                .FirstOrDefault(p => p.SubjectRef == request.SubjectRef && p.SectionId == ms.Id);

            int completedDuration = progress?.MinutesRead ?? 0;
            ProcessStatus processStatus = progress?.ProcessStatus ?? ProcessStatus.NotStarted;

            return new ModuleSectionDto(
                ms.Id,
                ms.Title,
                mediaUrls.TryGetValue(ms.MediaId, out var url) ? url : string.Empty,
                ms.Description,
                ms.TotalDuration,
                completedDuration,
                processStatus,
                ms.SectionArticles.Count
            );
        }).ToList();

        var paginatedResult = new PaginatedResult<ModuleSectionDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            dtoList
        );

        return new GetModuleSectionsResult(paginatedResult);
    }
}