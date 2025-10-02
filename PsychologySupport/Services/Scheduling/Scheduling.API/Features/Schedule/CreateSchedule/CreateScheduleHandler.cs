using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.Queries.LifeStyle;
using Mapster;
using MassTransit;
using Scheduling.API.Enums;
using Scheduling.API.Models;
using Scheduling.API.Services;
using Scheduling.API.Utils;

namespace Scheduling.API.Features.Schedule.CreateSchedule;

public record CreateScheduleCommand(
    Guid PatientId,
    Guid? DoctorId
) : ICommand<CreateScheduleResult>;

public record CreateScheduleResult(Guid? ScheduleId);

public class CreateScheduleHandler(
    SchedulingDbContext context,
    IRequestClient<GetAllActivitiesRequest> client,
    GeminiClient geminiClient
) : ICommandHandler<CreateScheduleCommand, CreateScheduleResult>
{
    public async Task<CreateScheduleResult> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = CreateSchedule(request.PatientId, request.DoctorId);
        context.Schedules.Add(schedule);

        var sessions = CreateSessions(schedule.Id, schedule.StartDate, schedule.EndDate, context);
        

        // Xây dựng options cho AI context từ schedule đã tạo
        var (entertainmentOptions, foodOptions, physicalOptions, therapeuticOptions) = BuildActivityOptions(sessions);

        // Lấy danh sách activities từ service ngoài (nếu cần)
        var activitiesResponse = (await FetchAllActivities(client, cancellationToken)).Activities;

        // Build AI context để truyền cho AI (nếu dùng)
        var aiScheduleContext = AiScheduleContextBuilder.BuildShortSchedule(
            sessions,
            entertainmentOptions,
            foodOptions,
            physicalOptions,
            therapeuticOptions
        );

        var optimizedSchedule  = await geminiClient.OptimizeScheduleAsync(request.PatientId, aiScheduleContext, activitiesResponse);

        MapOptimizedScheduleToEntity(schedule, optimizedSchedule);
        
        context.Sessions.AddRange(sessions);
        foreach (var session in sessions)
        {
            context.ScheduleActivities.AddRange(session.Activities);
        }
        
        await context.SaveChangesAsync(cancellationToken);
        
        return new CreateScheduleResult(schedule.Id);
    }

    private Models.Schedule CreateSchedule(Guid patientId, Guid? doctorId)
    {
        var startDate = DateTimeOffset.UtcNow.Date;
        var endDate = startDate.AddDays(6);
        return new Models.Schedule
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            DoctorId = doctorId,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    private List<Session> CreateSessions(Guid scheduleId, DateTimeOffset startDate, DateTimeOffset endDate, SchedulingDbContext context)
    {
        var sessions = new List<Session>();
        for (int i = 0; i < 7; i++)
        {
            var session = new Session
            {
                Id = Guid.NewGuid(),
                ScheduleId = scheduleId,
                Purpose = GetRandomPurpose(),
                Order = i + 1,
                StartDate = startDate.AddDays(i),
                EndDate = startDate.AddDays(i).AddHours(23).AddMinutes(59)
            };

            session.Activities = CreateActivitiesForSession(session, context);
            sessions.Add(session);
        }

        return sessions;
    }

    private List<ScheduleActivity> CreateActivitiesForSession(Session session, SchedulingDbContext context)
    {
        var activities = new List<ScheduleActivity>();
        var numActivities = Random.Shared.Next(3, 6);
        for (var j = 0; j < numActivities; j++)
        {
            var (entertainment, food, physical, therapeutic) = RandomActivityHelper.GetOneRandomActivity(context);

            var activity = new ScheduleActivity
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                EntertainmentActivityId = entertainment,
                FoodActivityId = food,
                PhysicalActivityId = physical,
                TherapeuticActivityId = therapeutic,
                Status = ScheduleActivityStatus.Pending,
                Description = $"Sample {j}",
                TimeRange = session.StartDate.AddHours(Random.Shared.Next(6, 22)),
                Duration = "60",
                DateNumber = session.Order
            };
            activities.Add(activity);
        }

        return activities;
    }

    private (List<ActivityOption> entertainment, List<ActivityOption> food, List<ActivityOption> physical, List<ActivityOption>
        therapeutic)
        BuildActivityOptions(List<Session> sessions)
    {
        var entertainmentOptions = sessions
            .SelectMany(s => s.Activities)
            .Where(a => a.EntertainmentActivityId != null)
            .Select(a => new ActivityOption(a.EntertainmentActivityId!.Value))
            .Distinct()
            .ToList();

        var foodOptions = sessions
            .SelectMany(s => s.Activities)
            .Where(a => a.FoodActivityId != null)
            .Select(a => new ActivityOption(a.FoodActivityId!.Value))
            .Distinct()
            .ToList();

        var physicalOptions = sessions
            .SelectMany(s => s.Activities)
            .Where(a => a.PhysicalActivityId != null)
            .Select(a => new ActivityOption(a.PhysicalActivityId!.Value))
            .Distinct()
            .ToList();

        var therapeuticOptions = sessions
            .SelectMany(s => s.Activities)
            .Where(a => a.TherapeuticActivityId != null)
            .Select(a => new ActivityOption(a.TherapeuticActivityId!.Value))
            .Distinct()
            .ToList();

        return (entertainmentOptions, foodOptions, physicalOptions, therapeuticOptions);
    }

    private async Task<GetAllActivitiesResponse> FetchAllActivities(IRequestClient<GetAllActivitiesRequest> client,
        CancellationToken cancellationToken)
    {
        var activitiesResponse =
            await client.GetResponse<GetAllActivitiesResponse>(new GetAllActivitiesRequest(), cancellationToken);
        return activitiesResponse.Message;
    }

    //---Helpers---
    private static class RandomActivityHelper
    {
        public static (Guid? Entertainment, Guid? Food, Guid? Physical, Guid? Therapeutic)
            GetOneRandomActivity(SchedulingDbContext context)
        {
            var type = Random.Shared.Next(0, 4);
            Guid? entertainment = null, food = null, physical = null, therapeutic = null;
            switch (type)
            {
                case 0:
                    entertainment = GetRandomActivityIdFromScheduleActivities("Entertainment", context);
                    break;
                case 1:
                    food = GetRandomActivityIdFromScheduleActivities("Food", context);
                    break;
                case 2:
                    physical = GetRandomActivityIdFromScheduleActivities("Physical", context);
                    break;
                case 3:
                    therapeutic = GetRandomActivityIdFromScheduleActivities("Therapeutic", context);
                    break;
            }

            return (entertainment, food, physical, therapeutic);
        }

        private static Guid? GetRandomActivityIdFromScheduleActivities(string activityType, SchedulingDbContext context)
        {
            switch (activityType)
            {
                case "Entertainment":
                    var entertainmentIds = context.ScheduleActivities
                        .Where(x => x.EntertainmentActivityId != null)
                        .Select(x => x.EntertainmentActivityId!.Value)
                        .Distinct()
                        .ToList();
                    return entertainmentIds.Any() ? entertainmentIds[Random.Shared.Next(entertainmentIds.Count)] : null;

                case "Food":
                    var foodIds = context.ScheduleActivities
                        .Where(x => x.FoodActivityId != null)
                        .Select(x => x.FoodActivityId!.Value)
                        .Distinct()
                        .ToList();
                    return foodIds.Any() ? foodIds[Random.Shared.Next(foodIds.Count)] : null;

                case "Physical":
                    var physicalIds = context.ScheduleActivities
                        .Where(x => x.PhysicalActivityId != null)
                        .Select(x => x.PhysicalActivityId!.Value)
                        .Distinct()
                        .ToList();
                    return physicalIds.Any() ? physicalIds[Random.Shared.Next(physicalIds.Count)] : null;

                case "Therapeutic":
                    var therapeuticIds = context.ScheduleActivities
                        .Where(x => x.TherapeuticActivityId != null)
                        .Select(x => x.TherapeuticActivityId!.Value)
                        .Distinct()
                        .ToList();
                    return therapeuticIds.Any() ? therapeuticIds[Random.Shared.Next(therapeuticIds.Count)] : null;

                default:
                    return null;
            }
        }
    }

    private static readonly string[] Purposes =
    [
        "General Consultation"
    ];

    private static string GetRandomPurpose()
    {
        return Purposes[Random.Shared.Next(Purposes.Length)];
    }
    
    private void MapOptimizedScheduleToEntity(Models.Schedule schedule, ShortSchedule optimized)
    {
        //1. Xoá hết activities cũ của từng session (tránh trùng)
        foreach (var session in schedule.Sessions)
        {
            session.Activities.Clear();
        }

        //2. Map lại từng session theo Order
        foreach (var shortSession in optimized.Sessions)
        {
            var targetSession = schedule.Sessions.FirstOrDefault(s => s.Order == shortSession.Order);
            if (targetSession == null) continue;

            //Map lại purpose, date
            targetSession.Purpose = shortSession.Purpose;
            targetSession.StartDate = shortSession.Date;
            targetSession.EndDate = shortSession.Date.AddHours(23).AddMinutes(59);

            //3. Thêm activity mới từ ShortActivity
            foreach (var shortActivity in shortSession.Activities)
            {
                var activity = new ScheduleActivity
                {
                    Id = Guid.NewGuid(),
                    SessionId = targetSession.Id,
                    Status = ScheduleActivityStatus.Pending,
                    Description = "None",
                    TimeRange = targetSession.StartDate.AddHours(Random.Shared.Next(6, 22)), //hoặc giữ cũ
                    Duration = "1h",
                    DateNumber = targetSession.Order
                };

                //Map FK đúng loại
                switch (shortActivity.Type)
                {
                    case "Entertainment": activity.EntertainmentActivityId = shortActivity.ActivityId; break;
                    case "Food": activity.FoodActivityId = shortActivity.ActivityId; break;
                    case "Physical": activity.PhysicalActivityId = shortActivity.ActivityId; break;
                    case "Therapeutic": activity.TherapeuticActivityId = shortActivity.ActivityId; break;
                }

                targetSession.Activities.Add(activity);
            }
        }
    }

}