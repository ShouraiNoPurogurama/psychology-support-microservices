using BuildingBlocks.CQRS;
using BuildingBlocks.Dtos;
using BuildingBlocks.Messaging.Events.LifeStyle;
using MassTransit;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.Schedule.GetScheduleActivity
{
    public record GetScheduleActivityQuery(Guid SessionId) : IQuery<GetScheduleActivityResult>;
    public record GetScheduleActivityResult(List<ScheduleActivityDto> ScheduleActivities);

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
                
                // Determine the activity type and request the activity details
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

}

