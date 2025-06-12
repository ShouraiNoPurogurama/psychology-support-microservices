using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features.PatientEntertainmentActivity.UpdatePatientEntertainmentActivities;

public record UpdatePatientEntertainmentActivitiesCommand(
    Guid PatientProfileId,
    List<(Guid EntertainmentActivityId, PreferenceLevel PreferenceLevel)> Activities)
    : ICommand<UpdatePatientEntertainmentActivitiesResult>;

public record UpdatePatientEntertainmentActivitiesResult(bool IsSucceeded);

public class UpdatePatientEntertainmentActivitiesHandler
    : ICommandHandler<UpdatePatientEntertainmentActivitiesCommand, UpdatePatientEntertainmentActivitiesResult>
{
    private readonly IRequestClient<PatientProfileExistenceRequest> _client;
    private readonly LifeStylesDbContext _context;

    public UpdatePatientEntertainmentActivitiesHandler(LifeStylesDbContext context,
        IRequestClient<PatientProfileExistenceRequest> client)
    {
        _context = context;
        _client = client;
    }

    public async Task<UpdatePatientEntertainmentActivitiesResult> Handle(
        UpdatePatientEntertainmentActivitiesCommand request, CancellationToken cancellationToken)
    {
        var response =
            await _client.GetResponse<PatientProfileExistenceResponse>(
                new PatientProfileExistenceRequest(request.PatientProfileId), cancellationToken);

        if (!response.Message.IsExist) throw new LifeStylesNotFoundException("PatientProfile", request.PatientProfileId);

        var existingActivities = _context.PatientEntertainmentActivities
            .Where(x => x.PatientProfileId == request.PatientProfileId);

        _context.PatientEntertainmentActivities.RemoveRange(existingActivities);

        var newActivities = request.Activities
            .Select(activity => new Models.PatientEntertainmentActivity
            {
                PatientProfileId = request.PatientProfileId,
                EntertainmentActivityId = activity.EntertainmentActivityId,
                PreferenceLevel = activity.PreferenceLevel
            });

        await _context.PatientEntertainmentActivities.AddRangeAsync(newActivities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new UpdatePatientEntertainmentActivitiesResult(true);
    }
}