using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos.Emotions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.Emotion;

public record GetAllEmotionQuery(
    PaginationRequest PaginationRequest,
    string? Search = null,
    string SortBy = "name",
    string SortOrder = "asc") : IQuery<GetAllEmotionResult>;

public record GetAllEmotionResult(PaginatedResult<EmotionDto> Emotions);

public class GetAllEmotionHandler (LifeStylesDbContext dbContext) : IQueryHandler<GetAllEmotionQuery, GetAllEmotionResult>
{
    public async Task<GetAllEmotionResult> Handle(GetAllEmotionQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Emotions.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(e => e.Name.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase));
        }
        
        switch (request.SortBy.ToLower())
        {
            case "name":
                query = request.SortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderBy(e => e.Name)
                    : query.OrderByDescending(e => e.Name);
                break;
            // case "createdAt": ...
            default:
                break;
        }

        
        var total = await query.LongCountAsync(cancellationToken: cancellationToken);
        
        var items = await query
            .Skip((request.PaginationRequest.PageIndex - 1) * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .ProjectToType<EmotionDto>()
            .ToListAsync(cancellationToken: cancellationToken);
        
        return new GetAllEmotionResult(
            new PaginatedResult<EmotionDto>(
                request.PaginationRequest.PageIndex,
                request.PaginationRequest.PageSize,
                total,
                items
            )
        );
    }
}