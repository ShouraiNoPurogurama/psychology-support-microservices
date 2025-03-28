using BuildingBlocks.CQRS;
using Scheduling.API.Dtos;
using Scheduling.API.Models;
using Scheduling.API.Enums;
using Newtonsoft.Json;


namespace Scheduling.API.Features.Schedule.ImportSchedule
{
    public record ImportScheduleCommand(Guid PatientId, string JsonData) : ICommand<ImportScheduleResult>;

    public record ImportScheduleResult(Guid ScheduleId);

    public class ImportScheduleHandler : ICommandHandler<ImportScheduleCommand, ImportScheduleResult>
    {
        private readonly SchedulingDbContext _context;

        public ImportScheduleHandler(SchedulingDbContext context)
        {
            _context = context;
        }

        public async Task<ImportScheduleResult> Handle(ImportScheduleCommand request, CancellationToken cancellationToken)
        {
            var data = request.JsonData.Replace("```json", "");
            data = data.Replace("```", "");
            var scheduleDto = JsonConvert.DeserializeObject<ScheduleDto>(data);

            if (scheduleDto == null)
            {
                throw new Exception("Invalid JSON data");
            }
            
            

            var schedule = new Models.Schedule
            {
                Id = scheduleDto.Id != Guid.Empty ? scheduleDto.Id : Guid.NewGuid(),
                PatientId = request.PatientId,
                DoctorId = Guid.Empty,
                StartDate = scheduleDto.StartDate.ToUniversalTime(),
                EndDate = scheduleDto.EndDate.ToUniversalTime()
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync(cancellationToken);
            
            var sessions = scheduleDto.Sessions.Select(sessionDto => new Session
            {
                Id = Guid.NewGuid(),
                ScheduleId = schedule.Id,
                Purpose = sessionDto.Purpose,
                Order = sessionDto.Order,
                StartDate = sessionDto.StartDate.ToUniversalTime(),
                EndDate = sessionDto.EndDate.ToUniversalTime()
            }).ToList();

            await _context.Sessions.AddRangeAsync(sessions, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            var activities = scheduleDto.Sessions.SelectMany(sessionDto => sessionDto.Activities.Select(activityDto => new ScheduleActivity
            {
                Id = activityDto.Id != Guid.Empty ? activityDto.Id : Guid.NewGuid(),
                SessionId = sessionDto.Id,
                Description = activityDto.Description,
                TimeRange = activityDto.TimeRange, 
                Duration = activityDto.Duration,   
                DateNumber = sessionDto.Order,
                Status = ScheduleActivityStatus.Pending,
                EntertainmentActivityId = activityDto.EntertainmentActivity?.Id,
                FoodActivityId = activityDto.FoodActivity?.Id,
                PhysicalActivityId = activityDto.PhysicalActivity?.Id,
                TherapeuticActivityId = activityDto.TherapeuticActivity?.Id
            })).ToList();


            await _context.ScheduleActivities.AddRangeAsync(activities, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new ImportScheduleResult(schedule.Id);
        }
    }
}