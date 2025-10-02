using Scheduling.API.Models;

namespace Scheduling.API.Utils;

public class AiScheduleContextBuilder
{
    public static ShortSchedule BuildShortSchedule(
        List<Session> sessions,
        List<ActivityOption> entertainment,
        List<ActivityOption> food,
        List<ActivityOption> physical,
        List<ActivityOption> therapeutic)
    {
        var shortSessions = sessions.Select(s => new ShortSession(
                s.Order,
                s.Purpose,
                s.StartDate,
                s.Activities.Select(a =>
                    {
                        if (a.EntertainmentActivityId != null)
                            return new ShortActivity("Entertainment", a.EntertainmentActivityId, a.TimeRange, a.Duration, a.DateNumber);
                        if (a.FoodActivityId != null)
                            return new ShortActivity("Food", a.FoodActivityId, a.TimeRange, a.Duration, a.DateNumber);
                        if (a.PhysicalActivityId != null)
                            return new ShortActivity("Physical", a.PhysicalActivityId, a.TimeRange, a.Duration, a.DateNumber);
                        if (a.TherapeuticActivityId != null)
                            return new ShortActivity("Therapeutic", a.TherapeuticActivityId, a.TimeRange, a.Duration, a.DateNumber);
                        return null!;
                    })
                    .ToList()
            ))
            .ToList();

        return new ShortSchedule(shortSessions);
    }
}

public record ShortActivity(
    string Type,
    Guid? ActivityId,
    DateTimeOffset TimeRange,
    string Duration,
    int DateNumber
);


public record ShortSession(
    int Order,
    string Purpose,
    DateTimeOffset Date,
    List<ShortActivity> Activities
);

public record ShortSchedule(
    List<ShortSession> Sessions
);

public record ActivityOption(Guid Id);