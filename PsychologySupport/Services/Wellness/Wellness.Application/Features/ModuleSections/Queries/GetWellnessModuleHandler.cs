using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Queries.Media;
using BuildingBlocks.Pagination;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Exceptions;
using Wellness.Application.Features.ModuleSections.Dtos;

public record GetWellnessModulesQuery(int PageIndex, int PageSize)
    : IQuery<GetWellnessModulesResult>;

public record GetWellnessModulesResult(PaginatedResult<WellnessModuleDto> Modules);

public class GetWellnessModuleHandler
    : IQueryHandler<GetWellnessModulesQuery, GetWellnessModulesResult>
{
    private readonly IWellnessDbContext _context;
    private readonly IRequestClient<GetMediaUrlRequest> _getMediaUrlClient;

    public GetWellnessModuleHandler(
        IWellnessDbContext context,
        IRequestClient<GetMediaUrlRequest> getMediaUrlClient)
    {
        _context = context;
        _getMediaUrlClient = getMediaUrlClient;
    }

    public async Task<GetWellnessModulesResult> Handle(GetWellnessModulesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WellnessModules
            .AsNoTracking()
            .Include(m => m.ModuleSections);

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
            throw new WellnessNotFoundException("Không tìm thấy module nào.");

        var modules = await query
            .OrderBy(m => m.Name)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Lấy MediaId
        var mediaIds = modules.Select(m => m.MediaId).Distinct().ToList();

        var mediaResponse = await _getMediaUrlClient
            .GetResponse<GetMediaUrlResponse>(new GetMediaUrlRequest { MediaIds = mediaIds }, cancellationToken);

        var mediaUrls = mediaResponse.Message.Urls;

        var dtoList = modules.Select(m => new WellnessModuleDto(
            m.Id,
            m.Name,
            mediaUrls.TryGetValue(m.MediaId, out var url) ? url : string.Empty,
            m.Description,
            m.ModuleSections.Count
        )).ToList();

        var paginatedResult = new PaginatedResult<WellnessModuleDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            dtoList
        );

        return new GetWellnessModulesResult(paginatedResult);
    }
}
