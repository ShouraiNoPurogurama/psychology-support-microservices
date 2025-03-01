using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.FoodActivity.GetFoodActivity;

public record GetFoodActivityQuery(Guid Id) : IQuery<GetFoodActivityResult>;

public record GetFoodActivityResult(FoodActivityDto FoodActivity);

public class GetFoodActivityHandler(LifeStylesDbContext context)
    : IQueryHandler<GetFoodActivityQuery, GetFoodActivityResult>
{
    public async Task<GetFoodActivityResult> Handle(GetFoodActivityQuery request, CancellationToken cancellationToken)
    {
        var activity = await context.FoodActivities
                           .Include(fa => fa.FoodNutrients)
                           .Include(fa => fa.FoodCategories)
                           .FirstOrDefaultAsync(fa => fa.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException("Food Activity", request.Id);

        var activityDto = new FoodActivityDto(
            activity.Id,
            activity.Name,
            activity.Description,
            activity.MealTime,
            activity.FoodNutrients.Select(fn => fn.Name),
            activity.FoodCategories.Select(fc => fc.Name),
            activity.IntensityLevel
        );

        return new GetFoodActivityResult(activityDto);
    }
}