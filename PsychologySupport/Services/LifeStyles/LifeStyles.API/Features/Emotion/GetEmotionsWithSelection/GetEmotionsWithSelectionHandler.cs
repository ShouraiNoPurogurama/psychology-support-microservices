using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.Queries.Translation;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos.Emotions;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.Emotion.GetEmotionsWithSelection;

public record GetEmotionsWithSelectionQuery(
    Guid profileId,
    PaginationRequest PaginationRequest,
    string? Search = null,
    string SortBy = "name",
    string SortOrder = "asc") : IQuery<GetEmotionsWithSelectionResult>;

public record GetEmotionsWithSelectionResult(PaginatedResult<EmotionWithSelectionDto> Emotions);

public class GetEmotionsWithSelectionHandler(
    LifeStylesDbContext dbContext,
    IRequestClient<GetTranslatedDataRequest> translationClient)
    : IQueryHandler<GetEmotionsWithSelectionQuery, GetEmotionsWithSelectionResult>
{
    public async Task<GetEmotionsWithSelectionResult> Handle(GetEmotionsWithSelectionQuery request,
        CancellationToken cancellationToken)
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

        var lastCheckpoint = await dbContext.PatientEmotionCheckpoints
            .Where(p => p.PatientProfileId == request.profileId)
            .OrderByDescending(e => e.LogDate)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        IQueryable<Guid> selectedEmotions;
        
        if (lastCheckpoint is null)
        {
            selectedEmotions = Enumerable.Empty<Guid>().AsQueryable();
        }
        else
        {
            selectedEmotions = dbContext.EmotionSelections
                .Where(e => e.EmotionCheckpointId == lastCheckpoint.Id)
                .Select(e => e.EmotionId);
        }


        var total = await query.LongCountAsync(cancellationToken: cancellationToken);

        var items = await query
            .Skip((request.PaginationRequest.PageIndex - 1) * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .Select(e => new EmotionWithSelectionDto(e.Id,
                e.Name.ToString(),
                selectedEmotions.Contains(e.Id)))
            .ToListAsync(cancellationToken: cancellationToken);

        var sortedItems = items
            .OrderByDescending(e => e.IsSelected)
            .ThenBy(e => e.Name);

        return new GetEmotionsWithSelectionResult(
            new PaginatedResult<EmotionWithSelectionDto>(
                request.PaginationRequest.PageIndex,
                request.PaginationRequest.PageSize,
                total,
                sortedItems
            )
        );
    }
}