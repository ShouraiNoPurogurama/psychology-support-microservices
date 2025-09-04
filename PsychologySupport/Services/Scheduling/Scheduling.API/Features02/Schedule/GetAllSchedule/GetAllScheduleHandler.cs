using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Dtos;
using MassTransit;
using BuildingBlocks.Dtos;
using BuildingBlocks.Messaging.Events.Queries.LifeStyle;

namespace Scheduling.API.Features02.Schedule.GetAllSchedule
{
    public record GetAllSchedulesQuery(
        int PageIndex,
        int PageSize,
        string? Search,
        string? SortBy,
        string? SortOrder,
        Guid? DoctorId,
    Guid? PatientId) : IQuery<GetAllSchedulesResult>;

    public record GetAllSchedulesResult(PaginatedResult<ScheduleDto> Schedules);

    public class GetAllScheduleHandler : IQueryHandler<GetAllSchedulesQuery, GetAllSchedulesResult>
    {
        private readonly SchedulingDbContext _context;
        private readonly IRequestClient<ActivityRequest> _activityClient;

        public GetAllScheduleHandler(SchedulingDbContext context, IRequestClient<ActivityRequest> activityClient)
        {
            _context = context;
            _activityClient = activityClient;
        }

        public async Task<GetAllSchedulesResult> Handle(
            GetAllSchedulesQuery request, CancellationToken cancellationToken)
        {
            var pageSize = request.PageSize;
            var pageIndex = Math.Max(0, request.PageIndex - 1);

            var query = _context.Schedules
                .Include(s => s.Sessions)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(schedule =>
                    (schedule.PatientId.ToString() == request.Search ||
                     schedule.DoctorId.ToString() == request.Search));
            }

            // Filtering by DoctorId and PatientId
            if (request.DoctorId.HasValue)
                query = query.Where(schedule => schedule.DoctorId == request.DoctorId);

            if (request.PatientId.HasValue)
                query = query.Where(schedule => schedule.PatientId == request.PatientId);

            // Sorting
            if (request.SortBy == "startDate")
            {
                query = request.SortOrder == "asc"
                    ? query.OrderBy(schedule => schedule.StartDate)
                    : query.OrderByDescending(schedule => schedule.StartDate);
            }
            else if (request.SortBy == "endDate")
            {
                query = request.SortOrder == "asc"
                    ? query.OrderBy(schedule => schedule.EndDate)
                    : query.OrderByDescending(schedule => schedule.EndDate);
            }
            else
            {
                query = query.OrderBy(s => s.StartDate);
            }

            // Pagination
            var totalCount = await query.CountAsync(cancellationToken);

            var schedules = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var paginatedResult = new PaginatedResult<ScheduleDto>(
                pageIndex + 1,
                pageSize,
                totalCount,
                schedules.Select(schedule => new ScheduleDto
                {
                    Id = schedule.Id,
                    PatientId = schedule.PatientId,
                    DoctorId = schedule.DoctorId,
                    StartDate = schedule.StartDate,
                    EndDate = schedule.EndDate,
                    TotalActivityCount = _context.ScheduleActivities
                        .Count(sa => schedule.Sessions.Select(s => s.Id).Contains(sa.SessionId)),
                    Sessions = schedule.Sessions.Select(async session =>
                    {
                        var activities = await GetActivities(session.Id, cancellationToken);

                        return new SessionDto
                        {
                            Id = session.Id,
                            ScheduleId = session.ScheduleId,
                            Purpose = session.Purpose,
                            Order = session.Order,
                            StartDate = session.StartDate,
                            EndDate = session.EndDate,
                            TotalActivityCompletedCount = activities.Count(a => a.Status == "Completed"),
                            Activities = activities
                        };
                    }).Select(t => t.Result).ToList()
                }).ToList()
            );

            return new GetAllSchedulesResult(paginatedResult);
        }

        private async Task<List<ScheduleActivityDto>> GetActivities(Guid sessionId, CancellationToken cancellationToken)
        {
            var scheduleActivitiesQuery = _context.ScheduleActivities
                     .Where(sa => sa.SessionId == sessionId);

            List<ScheduleActivitiesSpecificationDto> activities = [];

            var entertainmentActivities = scheduleActivitiesQuery
                .Where(sa => sa.EntertainmentActivityId.HasValue)
                .Select(sa => new ScheduleActivitiesSpecificationDto(sa, sa.EntertainmentActivityId!.Value))
                .ToList();

            entertainmentActivities.ForEach(a => activities.Add(a));

            var foodActivities = scheduleActivitiesQuery
                .Where(sa => sa.FoodActivityId.HasValue)
                .Select(sa => new ScheduleActivitiesSpecificationDto(sa, sa.FoodActivityId!.Value))
                .ToList();

            foodActivities.ForEach(a => activities.Add(a));

            var physicalActivities = scheduleActivitiesQuery
                .Where(sa => sa.PhysicalActivityId.HasValue)
                .Select(sa => new ScheduleActivitiesSpecificationDto(sa, sa.PhysicalActivityId!.Value))
                .ToList();

            physicalActivities.ForEach(a => activities.Add(a));

            var therapeuticActivities = scheduleActivitiesQuery
                .Where(sa => sa.TherapeuticActivityId.HasValue)
                .Select(sa => new ScheduleActivitiesSpecificationDto(sa, sa.TherapeuticActivityId!.Value))
                .ToList();

            therapeuticActivities.ForEach(a => activities.Add(a));

            var scheduleActivityDtos = new List<ScheduleActivityDto>();

            if (entertainmentActivities.Any())
            {
                var entertainmentResponse = await _activityClient
                    .GetResponse<ActivityRequestResponse<EntertainmentActivityDto>>(
                        new ActivityRequest(
                            entertainmentActivities.Select(e => e.SpecificActivityId).ToList(),
                            "Entertainment"), cancellationToken)
                    .ContinueWith(r => r.Result.Message.Activities, cancellationToken);

                entertainmentResponse.ForEach(activity =>
                {
                    var matchingActivity = activities.First(a => a.SpecificActivityId == activity.Id );
                    scheduleActivityDtos.Add(new ScheduleActivityDto
                    {
                        Id = activity.Id,
                        SessionId = sessionId,
                        Description = activity.Description,
                        TimeRange = matchingActivity.ScheduleActivity.TimeRange,
                        Duration = matchingActivity.ScheduleActivity.Duration,
                        DateNumber = matchingActivity.ScheduleActivity.DateNumber,
                        Status = matchingActivity.ScheduleActivity.Status.ToString(),
                        EntertainmentActivity = activity
                    });
                });
            }

            if (foodActivities.Any())
            {
                var foodResponse = await _activityClient.GetResponse<ActivityRequestResponse<FoodActivityDto>>(
                    new ActivityRequest(foodActivities.Select(e => e.SpecificActivityId).ToList(), "Food"), cancellationToken)
                    .ContinueWith(r => r.Result.Message.Activities, cancellationToken);

                foodResponse.ForEach(activity =>
                {
                    var matchingActivity = activities.First(a => a.SpecificActivityId == activity.Id);
                    scheduleActivityDtos.Add(new ScheduleActivityDto
                    {
                        Id = activity.Id,
                        SessionId = sessionId,
                        Description = activity.Description,
                        TimeRange = matchingActivity.ScheduleActivity.TimeRange,
                        Duration = matchingActivity.ScheduleActivity.Duration,
                        DateNumber = matchingActivity.ScheduleActivity.DateNumber,
                        Status = matchingActivity.ScheduleActivity.Status.ToString(),
                        FoodActivity = activity
                    });
                });
            }

            if (physicalActivities.Any())
            {
                var physicalResponse = await _activityClient.GetResponse<ActivityRequestResponse<PhysicalActivityDto>>(
                    new ActivityRequest(physicalActivities.Select(e => e.SpecificActivityId).ToList(), "Physical"), cancellationToken)
                    .ContinueWith(r => r.Result.Message.Activities, cancellationToken);

                physicalResponse.ForEach(activity =>
                {
                    var matchingActivity = activities.First(a => a.SpecificActivityId == activity.Id);
                    scheduleActivityDtos.Add(new ScheduleActivityDto
                    {
                        Id = activity.Id,
                        SessionId = sessionId,
                        Description = activity.Description,
                        TimeRange = matchingActivity.ScheduleActivity.TimeRange,
                        Duration = matchingActivity.ScheduleActivity.Duration,
                        DateNumber = matchingActivity.ScheduleActivity.DateNumber,
                        Status = matchingActivity.ScheduleActivity.Status.ToString(),
                        PhysicalActivity = activity
                    });
                });
            }

            if (therapeuticActivities.Any())
            {
                var therapeuticResponse = await _activityClient.GetResponse<ActivityRequestResponse<TherapeuticActivityDto>>(
                    new ActivityRequest(therapeuticActivities.Select(e => e.SpecificActivityId).ToList(), "Therapeutic"), cancellationToken)
                    .ContinueWith(r => r.Result.Message.Activities, cancellationToken);

                therapeuticResponse.ForEach(activity =>
                {
                    var matchingActivity = activities.First(a => a.SpecificActivityId == activity.Id);
                    scheduleActivityDtos.Add(new ScheduleActivityDto
                    {
                        Id = activity.Id,
                        SessionId = sessionId,
                        Description = activity.Description,
                        TimeRange = matchingActivity.ScheduleActivity.TimeRange,
                        Duration = matchingActivity.ScheduleActivity.Duration,
                        DateNumber = matchingActivity.ScheduleActivity.DateNumber,
                        Status = matchingActivity.ScheduleActivity.Status.ToString(),
                        TherapeuticActivity = activity
                    });
                });
            }

            return scheduleActivityDtos;
        }
    }
}