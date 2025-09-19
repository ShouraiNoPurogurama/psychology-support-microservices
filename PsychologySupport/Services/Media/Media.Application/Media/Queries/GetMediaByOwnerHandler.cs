using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Media.Application.Data;
using Media.Application.Dtos;
using Media.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Media.Application.Media.Queries;

public record GetMediaByOwnerQuery(
    MediaOwnerType OwnerType,
    Guid OwnerId,
    int PageIndex,
    int PageSize
) : IQuery<GetMediaByOwnerResult>;

public record GetMediaByOwnerResult(PaginatedResult<MediaByOwnerDto> Media);

public class GetMediaByOwnerHandler : IQueryHandler<GetMediaByOwnerQuery, GetMediaByOwnerResult>
{
    private readonly IMediaDbContext _context;

    public GetMediaByOwnerHandler(IMediaDbContext context)
    {
        _context = context;
    }

    public async Task<GetMediaByOwnerResult> Handle(GetMediaByOwnerQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MediaAssets
            .AsNoTracking()
            .Include(m => m.Owners)
            .Include(m => m.Variants)
            .Where(m => m.Owners.Any(o => o.MediaOwnerType == request.OwnerType && o.MediaOwnerId == request.OwnerId));

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            throw new NotFoundException($"No media found for owner_type '{request.OwnerType}' and owner_id '{request.OwnerId}'.");
        }

        var dtoList = await query
         .Skip((request.PageIndex - 1) * request.PageSize)
         .Take(request.PageSize)
         .Select(m => new MediaByOwnerDto(
             m.Id,
             m.State,
             m.Variants.Select(v => new MediaVariantByOwnerDto(
                 v.VariantType,
                 v.CdnUrl
             )).ToList()
         ))
        .ToListAsync(cancellationToken);


        var paginatedResult = new PaginatedResult<MediaByOwnerDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            dtoList
        );

        return new GetMediaByOwnerResult(paginatedResult);
    }
}