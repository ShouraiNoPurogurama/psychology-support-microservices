using BuildingBlocks.Dtos;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Queries.LifeStyle;
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
            var activityRequest = context.Message;
            var ids = activityRequest.Ids;

            switch (activityRequest.ActivityType)
            {
                case "Entertainment":
                    await RespondWithActivities(context, await GetEntertainmentActivities(ids));
                    break;
                case "Physical":
                    await RespondWithActivities(context, await GetPhysicalActivities(ids));
                    break;
                case "Food":
                    await RespondWithActivities(context, await GetFoodActivities(ids));
                    break;
                case "Therapeutic":
                    await RespondWithActivities(context, await GetTherapeuticActivities(ids));
                    break;
            }
        }

        private async Task RespondWithActivities<T>(ConsumeContext<ActivityRequest> context, List<T> activities)
         where T : IActivityDto
        {
            await context.RespondAsync(new ActivityRequestResponse<T>(activities));
        }


        private async Task<List<EntertainmentActivityDto>> GetEntertainmentActivities(List<Guid> ids)
        {
            var result = await _context.EntertainmentActivities
                .AsNoTracking()
                .Where(e => ids.Contains(e.Id))
                .Select(e => new EntertainmentActivityDto(e.Id, e.Name, e.Description, e.IntensityLevel, e.ImpactLevel))
                .ToListAsync();

            return result;
        }

        private async Task<List<PhysicalActivityDto>> GetPhysicalActivities(List<Guid> ids)
        {
            var result=  await _context.PhysicalActivities
                .AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .Select(p => new PhysicalActivityDto(p.Id, p.Name, p.Description, p.IntensityLevel, p.ImpactLevel.ToReadableString()))
                .ToListAsync();

            return result;
        }

        private async Task<List<FoodActivityDto>> GetFoodActivities(List<Guid> ids)
        {
            var result= await _context.FoodActivities
                .AsNoTracking()
                .Include(f => f.FoodNutrients)
                .Include(f => f.FoodCategories)
                .Where(f => ids.Contains(f.Id))
                .Select(f => new FoodActivityDto(
                    f.Id, f.Name, f.Description, f.MealTime,
                    f.FoodNutrients.Select(fn => fn.Name),
                    f.FoodCategories.Select(fc => fc.Name),
                    f.IntensityLevel))
                .ToListAsync();

            return result;
        }

        private async Task<List<TherapeuticActivityDto>> GetTherapeuticActivities(List<Guid> ids)
        {
            return await _context.TherapeuticActivities
                .AsNoTracking()
                .Where(t => ids.Contains(t.Id))
                .Select(t => new TherapeuticActivityDto(
                    t.Id, t.Name, t.Name, t.Description,
                    t.Instructions, t.IntensityLevel, t.ImpactLevel))
                .ToListAsync();
        }
    }
}