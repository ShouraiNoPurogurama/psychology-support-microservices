using BuildingBlocks.CQRS;
using Scheduling.API.Dtos;
using Scheduling.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace Scheduling.API.Features.Schedule.GetTotalSession
{
    public record GetTotalSessionQuery(Guid ScheduleId, DateOnly StartDate, DateOnly EndDate) : IQuery<List<TotalSessionDto>>;

    public class GetTotalSessionHandler : IQueryHandler<GetTotalSessionQuery, List<TotalSessionDto>>
    {
        private readonly SchedulingDbContext _context;

        public GetTotalSessionHandler(SchedulingDbContext context)
        {
            _context = context;
        }

        public async Task<List<TotalSessionDto>> Handle(GetTotalSessionQuery request, CancellationToken cancellationToken)
        {
            var activities = await _context.Schedules
                .Where(s => s.Id == request.ScheduleId)
                .SelectMany(s => s.Sessions.SelectMany(sess => sess.Activities))
                .Where(a => DateOnly.FromDateTime(a.TimeRange) >= request.StartDate
                         && DateOnly.FromDateTime(a.TimeRange) <= request.EndDate)
                .ToListAsync(cancellationToken);

            var sessions = activities
                .GroupBy(a => a.SessionId)
                .Select(g => new TotalSessionDto
                {
                    SessionId = g.Key,
                    Order = g.Min(a => DateOnly.FromDateTime(a.TimeRange)), 
                    Percentage = g.Any()
                        ? (float)g.Count(a => a.Status == ScheduleActivityStatus.Completed) / g.Count() * 100
                        : 0
                })
                .OrderBy(s => s.Order) 
                .ToList();

            return sessions;
        }
    }
}
