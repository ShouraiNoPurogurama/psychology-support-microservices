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
            IActivityDto activityDto = activityRequest.ActivityType switch
            {
                "Entertainment" => await GetEntertainmentActivity(activityRequest.Id),
                "Physical" => await GetPhysicalActivity(activityRequest.Id),
                "Food" => await GetFoodActivity(activityRequest.Id),
                "Therapeutic" => await GetTherapeuticActivity(activityRequest.Id),
                _ => throw new ArgumentException($"Unknown activity type: {activityRequest.ActivityType}")
            };

            await context.RespondAsync(new ActivityRequestResponse(activityDto));
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
