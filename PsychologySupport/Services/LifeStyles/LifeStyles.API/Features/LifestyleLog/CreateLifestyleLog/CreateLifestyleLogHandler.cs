using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features.LifestyleLog.CreateLifestyleLog;

public record CreateLifestyleLogCommand(
    Guid PatientProfileId,
    DateTimeOffset LogDate,
    SleepHoursLevel SleepHours,
    ExerciseFrequency ExerciseFrequency,
    AvailableTimePerDay AvailableTimePerDay
) : ICommand<CreateLifestyleLogResult>;

public record CreateLifestyleLogResult(bool IsSucceeded);
public class CreateLifestyleLogHandler
    : ICommandHandler<CreateLifestyleLogCommand, CreateLifestyleLogResult>
{
    private readonly LifeStylesDbContext _context;
    private readonly IRequestClient<PatientProfileExistenceRequest> _client;

    public CreateLifestyleLogHandler(LifeStylesDbContext context, IRequestClient<PatientProfileExistenceRequest> client)
    {
        _context = context;
        _client = client;
    }

    public async Task<CreateLifestyleLogResult> Handle(CreateLifestyleLogCommand request, CancellationToken cancellationToken)
    {
        var response =
            await _client.GetResponse<PatientProfileExistenceResponse>(
                new PatientProfileExistenceRequest(request.PatientProfileId), cancellationToken);

        if (!response.Message.IsExist)
            throw new LifeStylesNotFoundException("PatientProfile", request.PatientProfileId);

        var log = new Models.LifestyleLog
        {
            Id = Guid.NewGuid(),
            PatientProfileId = request.PatientProfileId,
            LogDate = request.LogDate,
            SleepHours = request.SleepHours,
            ExerciseFrequency = request.ExerciseFrequency,
            AvailableTimePerDay = request.AvailableTimePerDay
        };

        _context.LifestyleLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateLifestyleLogResult(true);
    }
}
