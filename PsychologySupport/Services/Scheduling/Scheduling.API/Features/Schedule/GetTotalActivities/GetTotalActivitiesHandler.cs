using BuildingBlocks.CQRS;
using Scheduling.API.Dtos;
using Scheduling.API.Enums;

namespace Scheduling.API.Features.Schedule.GetTotalActivities
{
    public record GetTotalActivitiesQuery(Guid ScheduleId, DateOnly StartDate, DateOnly EndDate) : IQuery<TotalActivityDto>;

    public class GetTotalActivitiesHandler : IQueryHandler<GetTotalActivitiesQuery, TotalActivityDto>
    {
        private readonly SchedulingDbContext _context;

        public GetTotalActivitiesHandler(SchedulingDbContext context)
        {
            _context = context;
        }

        public async Task<TotalActivityDto> Handle(GetTotalActivitiesQuery request, CancellationToken cancellationToken)
        {
            var activities = await _context.Schedules
                .Where(s => s.Id == request.ScheduleId)
                .SelectMany(s => s.Sessions.SelectMany(sess => sess.Activities))
                .Where(a => DateOnly.FromDateTime(a.TimeRange) >= request.StartDate
                    && DateOnly.FromDateTime(a.TimeRange) <= request.EndDate)
                .ToListAsync(cancellationToken);



            return new TotalActivityDto
            {
                EntertainmentActivityTime = 
                activities.Where(a => a.EntertainmentActivityId.HasValue && a.Status == ScheduleActivityStatus.Completed)
                           .Sum(a => int.Parse(a.Duration)),

                FoodActivityTime = 
                activities.Where(a => a.FoodActivityId.HasValue && a.Status == ScheduleActivityStatus.Completed)
                           .Sum(a => int.Parse(a.Duration)),

                PhysicalActivityTime = 
                activities.Where(a => a.PhysicalActivityId.HasValue && a.Status == ScheduleActivityStatus.Completed)
                           .Sum(a => int.Parse(a.Duration)),

                TherapeuticActivityTime = 
                activities.Where(a => a.TherapeuticActivityId.HasValue && a.Status == ScheduleActivityStatus.Completed)
                            .Sum(a => int.Parse(a.Duration))
            };
        }
    }
}
