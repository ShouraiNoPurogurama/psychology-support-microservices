using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.Schedule.GetAllSchedule
{
    
    public record GetAllSchedulesQuery(
        [FromQuery] int PageIndex,
        [FromQuery] int PageSize,
        [FromQuery] string? Search = "", 
        [FromQuery] string? SortBy = "startDate", // sort by startDate 
        [FromQuery] string? SortOrder = "asc", // asc or desc
        [FromQuery] Guid? DoctorId = null, // filter by doctor
        [FromQuery] Guid? PatientId = null) : IQuery<GetAllSchedulesResult>; // filter by patient
    public record GetAllSchedulesResult(PaginatedResult<ScheduleDto> Schedules);

    public class GetAllScheduleHandler : IQueryHandler<GetAllSchedulesQuery, GetAllSchedulesResult>
    {
        private readonly SchedulingDbContext _context;

        public GetAllScheduleHandler(SchedulingDbContext context)
        {
            _context = context;
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
                     schedule.DoctorId.ToString() == request.Search )
                );
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
                     Sessions = schedule.Sessions.Select(session => new SessionDto
                     {
                         Id = session.Id,
                         ScheduleId = session.ScheduleId,
                         Purpose = session.Purpose,
                         Order = session.Order,
                         StartDate = session.StartDate,
                         EndDate = session.EndDate
                     }).ToList()
                 }).ToList()
            );

            return new GetAllSchedulesResult(paginatedResult);
        }
    }

}
