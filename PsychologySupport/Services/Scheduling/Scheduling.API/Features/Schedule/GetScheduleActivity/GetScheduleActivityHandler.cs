using BuildingBlocks.CQRS;
using BuildingBlocks.Dtos;
using BuildingBlocks.Messaging.Events.LifeStyle;
using MassTransit;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.Schedule.GetScheduleActivity
{
    public record GetScheduleActivityQuery(Guid SessionId) : IQuery<GetScheduleActivityResult>;
    public record GetScheduleActivityResult(List<ScheduleActivityDto> ScheduleActivities);

    /*public class GetScheduleActivityHandler : IQueryHandler<GetScheduleActivityQuery, GetScheduleActivityResult>
    {
        private readonly SchedulingDbContext _context;
        private readonly IRequestClient<ActivityRequest> _activityClient;

        public GetScheduleActivityHandler(SchedulingDbContext context, IRequestClient<ActivityRequest> activityClient)
        {
            _context = context;
            _activityClient = activityClient;
        }

        public async Task<GetScheduleActivityResult> Handle(GetScheduleActivityQuery request, CancellationToken cancellationToken)
        {
            // Get the ScheduleActivity by sesionId
            var scheduleActivities = await _context.ScheduleActivities
                .Where(sa => sa.SessionId == request.SessionId)
                .ToListAsync(cancellationToken);

            var scheduleActivityDtos = new List<ScheduleActivityDto>();

            foreach (var scheduleActivity in scheduleActivities)
            {
                ScheduleActivityDto scheduleActivityDto = new ScheduleActivityDto
                {
                    SessionId = scheduleActivity.SessionId,
                    Description = scheduleActivity.Description,
                    TimeRange = scheduleActivity.TimeRange,
                    Duration = scheduleActivity.Duration,
                    DateNumber = scheduleActivity.DateNumber,
                    Status = scheduleActivity.Status.ToString()
                };


                if (scheduleActivity.EntertainmentActivityId.HasValue)
                {
                    var activityResponse = await _activityClient.GetResponse<ActivityRequestResponse<EntertainmentActivityDto>>(
                        new ActivityRequest(scheduleActivity.EntertainmentActivityId.Value, "Entertainment")
                    );
                    scheduleActivityDto.EntertainmentActivity = activityResponse.Message.Activity;
                }
                else if (scheduleActivity.FoodActivityId.HasValue)
                {
                    var activityResponse = await _activityClient.GetResponse<ActivityRequestResponse<FoodActivityDto>>(
                        new ActivityRequest(scheduleActivity.FoodActivityId.Value, "Food")
                    );
                    scheduleActivityDto.FoodActivity = activityResponse.Message.Activity;
                }
                else if (scheduleActivity.PhysicalActivityId.HasValue)
                {
                    var activityResponse = await _activityClient.GetResponse<ActivityRequestResponse<PhysicalActivityDto>>(
                        new ActivityRequest(scheduleActivity.PhysicalActivityId.Value, "Physical")
                    );
                    scheduleActivityDto.PhysicalActivity = activityResponse.Message.Activity;
                }
                else if (scheduleActivity.TherapeuticActivityId.HasValue)
                {
                    var activityResponse = await _activityClient.GetResponse<ActivityRequestResponse<TherapeuticActivityDto>>(
                        new ActivityRequest(scheduleActivity.TherapeuticActivityId.Value, "Therapeutic")
                    );
                    scheduleActivityDto.TherapeuticActivity = activityResponse.Message.Activity;
                }

                scheduleActivityDtos.Add(scheduleActivityDto);
            }

            return new GetScheduleActivityResult(scheduleActivityDtos);
        }
    }
*/

    public class GetScheduleActivityHandler : IQueryHandler<GetScheduleActivityQuery, GetScheduleActivityResult>
    {
        private readonly SchedulingDbContext _context;
        private readonly IRequestClient<ActivityRequest> _activityClient;

        public GetScheduleActivityHandler(SchedulingDbContext context, IRequestClient<ActivityRequest> activityClient)
        {
            _context = context;
            _activityClient = activityClient;
        }

        public async Task<GetScheduleActivityResult> Handle(GetScheduleActivityQuery request, CancellationToken cancellationToken)
        {
            var scheduleActivities = await _context.ScheduleActivities
                .Where(sa => sa.SessionId == request.SessionId)
                .ToListAsync(cancellationToken);

            var activityRequests = new Dictionary<string, List<Guid>>
        {
            { "Entertainment", scheduleActivities.Where(sa => sa.EntertainmentActivityId.HasValue).Select(sa => sa.EntertainmentActivityId.Value).ToList() },
            { "Food", scheduleActivities.Where(sa => sa.FoodActivityId.HasValue).Select(sa => sa.FoodActivityId.Value).ToList() },
            { "Physical", scheduleActivities.Where(sa => sa.PhysicalActivityId.HasValue).Select(sa => sa.PhysicalActivityId.Value).ToList() },
            { "Therapeutic", scheduleActivities.Where(sa => sa.TherapeuticActivityId.HasValue).Select(sa => sa.TherapeuticActivityId.Value).ToList() }
        };

            var activityTasks = activityRequests
                .Where(kv => kv.Value.Any())
                .ToDictionary(
                    kv => kv.Key,
                    kv => _activityClient.GetResponse<ActivityRequestResponse<IActivityDto>>(
                        new ActivityRequest(kv.Value, kv.Key)));

            await Task.WhenAll(activityTasks.Values);

            var activityResponses = activityTasks.ToDictionary(
                task => task.Key,
                task => task.Value.Result.Message.Activities);

            var scheduleActivityDtos = scheduleActivities.Select(scheduleActivity => new ScheduleActivityDto
            {
                SessionId = scheduleActivity.SessionId,
                Description = scheduleActivity.Description,
                TimeRange = scheduleActivity.TimeRange,
                Duration = scheduleActivity.Duration,
                DateNumber = scheduleActivity.DateNumber,
                Status = scheduleActivity.Status.ToString(),
                EntertainmentActivity = scheduleActivity.EntertainmentActivityId.HasValue && activityResponses.ContainsKey("Entertainment")
                    ? activityResponses["Entertainment"].OfType<EntertainmentActivityDto>()
                        .FirstOrDefault(a => a.Id == scheduleActivity.EntertainmentActivityId.Value)
                    : null,
                FoodActivity = scheduleActivity.FoodActivityId.HasValue && activityResponses.ContainsKey("Food")
                    ? activityResponses["Food"].OfType<FoodActivityDto>()
                        .FirstOrDefault(a => a.Id == scheduleActivity.FoodActivityId.Value)
                    : null,
                PhysicalActivity = scheduleActivity.PhysicalActivityId.HasValue && activityResponses.ContainsKey("Physical")
                    ? activityResponses["Physical"].OfType<PhysicalActivityDto>()
                        .FirstOrDefault(a => a.Id == scheduleActivity.PhysicalActivityId.Value)
                    : null,
                TherapeuticActivity = scheduleActivity.TherapeuticActivityId.HasValue && activityResponses.ContainsKey("Therapeutic")
                    ? activityResponses["Therapeutic"].OfType<TherapeuticActivityDto>()
                        .FirstOrDefault(a => a.Id == scheduleActivity.TherapeuticActivityId.Value)
                    : null
            }).ToList();

            return new GetScheduleActivityResult(scheduleActivityDtos);
        }
    }
}

