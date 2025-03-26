using BuildingBlocks.CQRS;
using Scheduling.API.Dtos;
using Scheduling.API.Models;

namespace Scheduling.API.Features.Schedule.ImportSchedule
{
    public record ImportScheduleCommand(string JsonFilePath) : ICommand<ImportScheduleResult>;

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
            string jsonData = await File.ReadAllTextAsync(request.JsonFilePath);
            var scheduleDto = JsonConvert.DeserializeObject<ScheduleDto>(jsonData);

            if (scheduleDto == null)
            {
                throw new Exception("Invalid JSON data");
            }

            var schedule = new Schedule
            {
                Id = scheduleDto.Id != Guid.Empty ? scheduleDto.Id : Guid.NewGuid(),
                PatientId = scheduleDto.PatientId,
                DoctorId = scheduleDto.DoctorId,
                StartDate = scheduleDto.StartDate,
                EndDate = scheduleDto.EndDate,
            };

            _context.Schedules.Add(schedule);

            var sessions = new List<Session>();
            foreach (var sessionDto in scheduleDto.Sessions)
            {
                var session = new Session
                {
                    Id = sessionDto.Id != Guid.Empty ? sessionDto.Id : Guid.NewGuid(),
                    ScheduleId = schedule.Id,
                    Purpose = sessionDto.Purpose,
                    Order = sessionDto.Order,
                    StartDate = sessionDto.StartDate,
                    EndDate = sessionDto.EndDate
                };
                sessions.Add(session);
            }

            await _context.Sessions.AddRangeAsync(sessions);

            var activities = new List<ScheduleActivity>();
            foreach (var sessionDto in scheduleDto.Sessions)
            {
                foreach (var activityDto in sessionDto.Activities)
                {
                    var activity = new ScheduleActivity
                    {
                        Id = activityDto.Id != Guid.Empty ? activityDto.Id : Guid.NewGuid(),
                        SessionId = sessionDto.Id,
                        Description = activityDto.Purpose,
                        TimeRange = activityDto.StartDate,
                        Duration = (activityDto.EndDate - activityDto.StartDate).ToString(),
                        DateNumber = activityDto.Order,
                        Status = Scheduling.API.Enums.ScheduleActivityStatus.Pending
                    };
                    activities.Add(activity);
                }
            }

            await _context.ScheduleActivities.AddRangeAsync(activities);
            await _context.SaveChangesAsync(cancellationToken);

            return new ImportScheduleResult(schedule.Id);
        }
    }
}
