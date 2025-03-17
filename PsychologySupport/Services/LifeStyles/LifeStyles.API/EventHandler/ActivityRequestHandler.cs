using BuildingBlocks.Dtos;
using BuildingBlocks.Messaging.Events.LifeStyle;
using LifeStyles.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.EventHandler
{
    /*   public class ActivityRequestHandler : IConsumer<ActivityRequest>
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

       }*/

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

            switch (activityRequest.ActivityType)
            {
                case "Entertainment":
                {
                    var result = await GetEntertainmentActivities(activityRequest.Ids);
                    await context.RespondAsync(new ActivityRequestResponse<EntertainmentActivityDto>(result));
                    return;
                }
                case "Physical":
                {
                    var result = await GetPhysicalActivities(activityRequest.Ids);
                    await context.RespondAsync(new ActivityRequestResponse<PhysicalActivityDto>(result));
                    return;
                }
                case "Food":
                {
                    var result = await GetFoodActivities(activityRequest.Ids);
                    await context.RespondAsync(new ActivityRequestResponse<FoodActivityDto>(result));
                    return;
                }
                case "Therapeutic":
                {
                    var result = await GetTherapeuticActivities(activityRequest.Ids);
                    await context.RespondAsync(new ActivityRequestResponse<TherapeuticActivityDto>(result));
                    return;
                }
            }
        }

        private async Task<List<EntertainmentActivityDto>> GetEntertainmentActivities(List<Guid> ids)
        {
            var result = await _context.EntertainmentActivities
                .Where(e => ids.Contains(e.Id))
                .Select(e => new EntertainmentActivityDto(e.Id, e.Name, e.Description, e.IntensityLevel, e.ImpactLevel))
                .ToListAsync();

            return result;
        }

        private async Task<List<PhysicalActivityDto>> GetPhysicalActivities(List<Guid> ids)
        {
            var result=  await _context.PhysicalActivities
                .Where(p => ids.Contains(p.Id))
                .Select(p => new PhysicalActivityDto(p.Id, p.Name, p.Description, p.IntensityLevel, p.ImpactLevel))
                .ToListAsync();

            return result;
        }

        private async Task<List<FoodActivityDto>> GetFoodActivities(List<Guid> ids)
        {
            var result= await _context.FoodActivities
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
                .Where(t => ids.Contains(t.Id))
                .Select(t => new TherapeuticActivityDto(
                    t.Id, t.Name, t.Name, t.Description,
                    t.Instructions, t.IntensityLevel, t.ImpactLevel))
                .ToListAsync();
        }
    }

    /* public class ActivityRequestHandler : IConsumer<ActivityRequest>
     {
         private readonly LifeStylesDbContext _context;

         public ActivityRequestHandler(LifeStylesDbContext context)
         {
             _context = context;
         }

         public async Task Consume(ConsumeContext<ActivityRequest> context)
         {
             var activityRequest = context.Message;
             object response = activityRequest.ActivityType switch
             {
                 "Entertainment" => await GetActivity<EntertainmentActivity, EntertainmentActivityDto>(
                     _context.EntertainmentActivities, entity =>
                     new EntertainmentActivityDto(entity.Id, entity.Name, entity.Description, entity.IntensityLevel, entity.ImpactLevel)),

                 "Physical" => await GetActivity<PhysicalActivity, PhysicalActivityDto>(
                     _context.PhysicalActivities, entity =>
                     new PhysicalActivityDto(entity.Id, entity.Name, entity.Description, entity.IntensityLevel, entity.ImpactLevel)),

                 "Food" => await GetActivity<FoodActivity, FoodActivityDto>(
                     _context.FoodActivities.Include(f => f.FoodNutrients).Include(f => f.FoodCategories), entity =>
                     new FoodActivityDto(entity.Id, entity.Name, entity.Description, entity.MealTime,
                         entity.FoodNutrients.Select(fn => fn.Name), entity.FoodCategories.Select(fc => fc.Name), entity.IntensityLevel)),

                 "Therapeutic" => await GetActivity<TherapeuticActivity, TherapeuticActivityDto>(
                     _context.TherapeuticActivities, entity =>
                     new TherapeuticActivityDto(entity.Id, entity.TherapeuticType.Name,entity.Name, entity.Description, entity.Instructions, entity.IntensityLevel, entity.ImpactLevel)),

                 _ => throw new ArgumentException($"Unknown activity type: {activityRequest.ActivityType}")
             };

             if (response != null)
                 await context.RespondAsync(response);
         }

         private async Task<TDto> GetActivity<TEntity, TDto>(IQueryable<TEntity> query, Func<TEntity, TDto> map) where TEntity : class
         {
             var entity = await query.SingleOrDefaultAsync();
             return entity != null ? map(entity) : default;
         }
     }*/
}