using BuildingBlocks.Dtos;
using BuildingBlocks.Messaging.Events.LifeStyle;
using LifeStyles.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.EventHandler
{

    public class ActivityRequestHandler : IConsumer<ActivityRequest>
    {
        private readonly LifeStylesDbContext _context;

        public ActivityRequestHandler(LifeStylesDbContext context)
        {
            _context = context;
        }
        public async Task Consume(ConsumeContext<ActivityRequest> context)
        {
            // Extract the request data
            var activityRequest = context.Message;

            // Depending on ActivityType, fetch the appropriate entity
            switch (activityRequest.ActivityType)
            {
                case "Entertainment":
                    var entertainmentActivity = await GetEntertainmentActivity(activityRequest.Id);
                    await context.RespondAsync(new ActivityRequestResponse<EntertainmentActivityDto>(entertainmentActivity));
                    break;

                case "Physical":
                    var physicalActivity = await GetPhysicalActivity(activityRequest.Id);
                    await context.RespondAsync(new ActivityRequestResponse<PhysicalActivityDto>(physicalActivity));
                    break;

                case "Food":
                    var foodActivity = await GetFoodActivity(activityRequest.Id);
                    await context.RespondAsync(new ActivityRequestResponse<FoodActivityDto>(foodActivity));
                    break;

                case "Therapeutic":
                    var therapeuticActivity = await GetTherapeuticActivity(activityRequest.Id);
                    await context.RespondAsync(new ActivityRequestResponse<TherapeuticActivityDto>(therapeuticActivity));
                    break;

                default:
                    throw new ArgumentException($"Unknown activity type: {activityRequest.ActivityType}");
            }
        }


        private async Task<EntertainmentActivityDto> GetEntertainmentActivity(Guid id)
        {
            var activity = await _context.EntertainmentActivities
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();

            
            return new EntertainmentActivityDto(
                activity.Id,
                activity.Name,
                activity.Description,
                activity.IntensityLevel,
                activity.ImpactLevel);
        }

        private async Task<PhysicalActivityDto> GetPhysicalActivity(Guid id)
        {
            var activity = await _context.PhysicalActivities
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

           
            return new PhysicalActivityDto(
                activity.Id,
                activity.Name,
                activity.Description,
                activity.IntensityLevel,
                activity.ImpactLevel);
        }

        private async Task<FoodActivityDto> GetFoodActivity(Guid id)
        {
            var activity = await _context.FoodActivities
                .Include(f => f.FoodNutrients)
                .Include(f => f.FoodCategories)
                .Where(f => f.Id == id)
                .FirstOrDefaultAsync();

          
            return new FoodActivityDto(
                activity.Id,
                activity.Name,
                activity.Description,
                activity.MealTime,
                activity.FoodNutrients.Select(fn => fn.Name), 
                activity.FoodCategories.Select(fc => fc.Name), 
                activity.IntensityLevel);
        }

        private async Task<TherapeuticActivityDto> GetTherapeuticActivity(Guid id)
        {
            var activity = await _context.TherapeuticActivities
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();

        
            return new TherapeuticActivityDto(
                activity.Id,
                activity.Name,
                activity.Name,
                activity.Description,
                activity.Instructions,
                activity.IntensityLevel,
                activity.ImpactLevel);
        }

    }
}
