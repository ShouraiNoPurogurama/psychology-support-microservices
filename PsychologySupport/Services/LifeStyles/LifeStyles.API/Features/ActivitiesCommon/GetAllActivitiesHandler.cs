using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Dtos.LifeStyles;
using LifeStyles.API.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.ActivitiesCommon;

public record GetAllActivitiesQuery : IQuery<GetAllActivitiesResult>;

public record GetAllActivitiesResult(IEnumerable<ActivityDto> Activities);

public class GetAllActivitiesHandler(LifeStylesDbContext dbContext)
    : IQueryHandler<GetAllActivitiesQuery, GetAllActivitiesResult>
{
    public async Task<GetAllActivitiesResult> Handle(GetAllActivitiesQuery request, CancellationToken cancellationToken)
    {
        var entertainmentActivities = await dbContext.EntertainmentActivities
            .Select(x => new ActivityDto(
                "Entertainment",
                x.Id,
                x.Name
            ))
            .ToListAsync(cancellationToken);

        var foodActivities = await dbContext.FoodActivities
            .Select(x => new ActivityDto(
                "Food",
                x.Id,
                x.Name
            ))
            .ToListAsync(cancellationToken);

        var physicalActivities = await dbContext.PhysicalActivities
            .Select(x => new ActivityDto(
                "Physical",
                x.Id,
                x.Name
            ))
            .ToListAsync(cancellationToken);

        var therapeuticActivities = await dbContext.TherapeuticActivities
            .Select(x => new ActivityDto(
                "Therapeutic",
                x.Id,
                x.Name
            ))
            .ToListAsync(cancellationToken);

        var allActivities = entertainmentActivities
            .Concat(foodActivities)
            .Concat(physicalActivities)
            .Concat(therapeuticActivities)
            .ToList();

        return new GetAllActivitiesResult(allActivities);
    }
}