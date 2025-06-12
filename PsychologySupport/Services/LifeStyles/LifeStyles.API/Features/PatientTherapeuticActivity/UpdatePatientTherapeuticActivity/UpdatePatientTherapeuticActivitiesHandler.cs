using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features.PatientTherapeuticActivity.UpdatePatientTherapeuticActivity;

public record UpdatePatientTherapeuticActivitiesCommand(
    Guid PatientProfileId,
    List<(Guid TherapeuticActivityId, PreferenceLevel PreferenceLevel)> Activities)
    : ICommand<UpdatePatientTherapeuticActivitiesResult>;

public record UpdatePatientTherapeuticActivitiesResult(bool IsSucceeded);

public class UpdatePatientTherapeuticActivitiesHandler
    : ICommandHandler<UpdatePatientTherapeuticActivitiesCommand, UpdatePatientTherapeuticActivitiesResult>
{
    private readonly IRequestClient<PatientProfileExistenceRequest> _client;
    private readonly LifeStylesDbContext _context;

    public UpdatePatientTherapeuticActivitiesHandler(LifeStylesDbContext context,
        IRequestClient<PatientProfileExistenceRequest> client)
    {
        _context = context;
        _client = client;
    }

    public async Task<UpdatePatientTherapeuticActivitiesResult> Handle(
        UpdatePatientTherapeuticActivitiesCommand request, CancellationToken cancellationToken)
    {
        var response =
            await _client.GetResponse<PatientProfileExistenceResponse>(
                new PatientProfileExistenceRequest(request.PatientProfileId), cancellationToken);

        if (!response.Message.IsExist) throw new LifeStylesNotFoundException("PatientProfile", request.PatientProfileId);

        var existingActivities = _context.PatientTherapeuticActivities
            .Where(x => x.PatientProfileId == request.PatientProfileId);

        _context.PatientTherapeuticActivities.RemoveRange(existingActivities);

        var newActivities = request.Activities
            .Select(activity => new Models.PatientTherapeuticActivity
            {
                PatientProfileId = request.PatientProfileId,
                TherapeuticActivityId = activity.TherapeuticActivityId,
                PreferenceLevel = activity.PreferenceLevel
            });

        await _context.PatientTherapeuticActivities.AddRangeAsync(newActivities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new UpdatePatientTherapeuticActivitiesResult(true);
    }
}