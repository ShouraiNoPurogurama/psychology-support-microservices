using BuildingBlocks.CQRS;
using BuildingBlocks.Dtos;
using BuildingBlocks.Messaging.Events.Queries.LifeStyle;
using MassTransit;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.Schedule.GetScheduleActivity;

public record GetScheduleActivityQuery(Guid SessionId) : IQuery<GetScheduleActivityResult>;

public record GetScheduleActivityResult(List<ScheduleActivityDto> ScheduleActivities);

public class GetScheduleActivityHandler(SchedulingDbContext context, IRequestClient<ActivityRequest> activityClient)
    : IQueryHandler<GetScheduleActivityQuery, GetScheduleActivityResult>
{
    public async Task<GetScheduleActivityResult> Handle(GetScheduleActivityQuery request, CancellationToken cancellationToken)
    {
        var scheduleActivitiesQuery = context.ScheduleActivities
            .Where(sa => sa.SessionId == request.SessionId)
            .AsQueryable();

        List<ScheduleActivitiesSpecificationDto> activities = [];

        var entertainmentActivities = scheduleActivitiesQuery
            .Where(sa => sa.EntertainmentActivityId.HasValue)
            .Select(sa => new ScheduleActivitiesSpecificationDto(sa, sa.EntertainmentActivityId!.Value))
            .ToList();

        entertainmentActivities.ForEach(a => activities.Add(a));

        var foodActivities = scheduleActivitiesQuery
            .Where(sa => sa.FoodActivityId.HasValue)
            .Select(sa =>
                new ScheduleActivitiesSpecificationDto(sa, sa.FoodActivityId!.Value))
            .ToList();

        foodActivities.ForEach(a => activities.Add(a));

        var physicalActivities = scheduleActivitiesQuery
            .Where(sa => sa.PhysicalActivityId.HasValue)
            .Select(sa =>
                new ScheduleActivitiesSpecificationDto(sa, sa.PhysicalActivityId!.Value))
            .ToList();
        physicalActivities.ForEach(a => activities.Add(a));

        var therapeuticActivities = scheduleActivitiesQuery
            .Where(sa => sa.TherapeuticActivityId.HasValue)
            .Select(sa =>
                new ScheduleActivitiesSpecificationDto(sa, sa.TherapeuticActivityId!.Value))
            .ToList();

        therapeuticActivities.ForEach(a => activities.Add(a));

        var scheduleActivityDtos = new List<ScheduleActivityDto>();

        var entertainmentActivitiesResponse = await activityClient
            .GetResponse<ActivityRequestResponse<EntertainmentActivityDto>>(
                new ActivityRequest(
                    entertainmentActivities.Select(e => e.SpecificActivityId).ToList(),
                    "Entertainment"
                ), cancellationToken)
            .ContinueWith(r => r.Result.Message.Activities, cancellationToken);

        entertainmentActivitiesResponse.ForEach(activity =>
        {
            var matchingActivity = activities.First(a => a.SpecificActivityId == activity.Id);
            scheduleActivityDtos.Add(new ScheduleActivityDto
            {
                Id = activity.Id,
                SessionId = request.SessionId,
                Description = activity.Description,
                TimeRange = matchingActivity.ScheduleActivity.TimeRange,
                Duration = matchingActivity.ScheduleActivity.Duration,
                DateNumber = matchingActivity.ScheduleActivity.DateNumber,
                Status = matchingActivity.ScheduleActivity.Status.ToString(),
                EntertainmentActivity = activity
            });
        });

        var foodActivitiesResponse = await activityClient.GetResponse<ActivityRequestResponse<FoodActivityDto>>(
                new ActivityRequest(foodActivities.Select(e => e.SpecificActivityId).ToList(),
                    "Food"), cancellationToken)
            .ContinueWith(r => r.Result.Message.Activities, cancellationToken);

        foodActivitiesResponse.ForEach(activity =>
        {
            var matchingActivity = activities.First(a => a.SpecificActivityId == activity.Id);
            scheduleActivityDtos.Add(new ScheduleActivityDto
            {
                Id = activity.Id,
                SessionId = request.SessionId,
                Description = activity.Description,
                TimeRange = matchingActivity.ScheduleActivity.TimeRange,
                Duration = matchingActivity.ScheduleActivity.Duration,
                DateNumber = matchingActivity.ScheduleActivity.DateNumber,
                Status = matchingActivity.ScheduleActivity.Status.ToString(),
                FoodActivity = activity
            });
        });

        var physicalActivitiesResponse = await activityClient.GetResponse<ActivityRequestResponse<PhysicalActivityDto>>(
                new ActivityRequest(physicalActivities.Select(e => e.SpecificActivityId).ToList(),
                    "Physical"), cancellationToken)
            .ContinueWith(r => r.Result.Message.Activities, cancellationToken);

        physicalActivitiesResponse.ForEach(activity =>
        {
            var matchingActivity = activities.First(a => a.SpecificActivityId == activity.Id);
            scheduleActivityDtos.Add(new ScheduleActivityDto
            {
                Id = activity.Id,
                SessionId = request.SessionId,
                Description = activity.Description,
                TimeRange = matchingActivity.ScheduleActivity.TimeRange,
                Duration = matchingActivity.ScheduleActivity.Duration,
                DateNumber = matchingActivity.ScheduleActivity.DateNumber,
                Status = matchingActivity.ScheduleActivity.Status.ToString(),
                PhysicalActivity = activity
            });
        });

        var therapeuticActivitiesResponse = await activityClient
            .GetResponse<ActivityRequestResponse<TherapeuticActivityDto>>(
                new ActivityRequest(therapeuticActivities.Select(e => e.SpecificActivityId).ToList(),
                    "Therapeutic"), cancellationToken)
            .ContinueWith(r => r.Result.Message.Activities, cancellationToken);

        therapeuticActivitiesResponse.ForEach(activity =>
        {
            var matchingActivity = activities.First(a => a.SpecificActivityId == activity.Id);
            scheduleActivityDtos.Add(new ScheduleActivityDto
            {
                Id = activity.Id,
                SessionId = request.SessionId,
                Description = activity.Description,
                TimeRange = matchingActivity.ScheduleActivity.TimeRange,
                Duration = matchingActivity.ScheduleActivity.Duration,
                DateNumber = matchingActivity.ScheduleActivity.DateNumber,
                Status = matchingActivity.ScheduleActivity.Status.ToString(),
                TherapeuticActivity = activity
            });
        });


        var result = scheduleActivityDtos
            .OrderBy(s => s.TimeRange)
            .ToList();
        
        return new GetScheduleActivityResult(result);
    }
}