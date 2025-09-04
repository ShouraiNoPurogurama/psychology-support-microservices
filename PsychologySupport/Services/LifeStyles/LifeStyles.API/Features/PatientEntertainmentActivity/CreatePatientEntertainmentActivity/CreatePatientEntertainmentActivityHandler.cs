using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features.PatientEntertainmentActivity.CreatePatientEntertainmentActivity;

public record CreatePatientEntertainmentActivityCommand(
    Guid PatientProfileId,
    List<(Guid EntertainmentActivityId, PreferenceLevel PreferenceLevel)> Activities)
    : ICommand<CreatePatientEntertainmentActivityResult>;

public record CreatePatientEntertainmentActivityResult(bool IsSucceeded);

public class CreatePatientEntertainmentActivityHandler
    : ICommandHandler<CreatePatientEntertainmentActivityCommand, CreatePatientEntertainmentActivityResult>
{
    private readonly IRequestClient<PatientProfileExistenceRequest> _client;
    private readonly LifeStylesDbContext _context;

    public CreatePatientEntertainmentActivityHandler(LifeStylesDbContext context,
        IRequestClient<PatientProfileExistenceRequest> client)
    {
        _context = context;
        _client = client;
    }

    public async Task<CreatePatientEntertainmentActivityResult> Handle(
        CreatePatientEntertainmentActivityCommand request, CancellationToken cancellationToken)
    {
        var response =
            await _client.GetResponse<PatientProfileExistenceResponse>(
                new PatientProfileExistenceRequest(request.PatientProfileId), cancellationToken);

        if (!response.Message.IsExist) throw new LifeStylesNotFoundException("PatientProfile", request.PatientProfileId);

        var activities = request.Activities
            .Select(activity => new Models.PatientEntertainmentActivity
            {
                PatientProfileId = request.PatientProfileId,
                EntertainmentActivityId = activity.EntertainmentActivityId,
                PreferenceLevel = activity.PreferenceLevel
            })
            .ToList();

        _context.PatientEntertainmentActivities.AddRange(activities);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreatePatientEntertainmentActivityResult(true);
    }
}