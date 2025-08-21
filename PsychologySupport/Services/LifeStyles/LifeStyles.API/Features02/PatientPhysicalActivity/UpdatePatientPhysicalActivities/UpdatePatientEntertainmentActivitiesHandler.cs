using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features02.PatientPhysicalActivity.UpdatePatientPhysicalActivities;

public record UpdatePatientPhysicalActivitiesV2Command(
    Guid PatientProfileId,
    List<(Guid PhysicalActivityId, PreferenceLevel PreferenceLevel)> Activities)
    : ICommand<UpdatePatientPhysicalActivitiesV2Result>;

public record UpdatePatientPhysicalActivitiesV2Result(bool IsSucceeded);

public class UpdatePatientPhysicalActivitiesV2Handler
    : ICommandHandler<UpdatePatientPhysicalActivitiesV2Command, UpdatePatientPhysicalActivitiesV2Result>
{
    private readonly IRequestClient<PatientProfileExistenceRequest> _client;
    private readonly LifeStylesDbContext _context;

    public UpdatePatientPhysicalActivitiesV2Handler(LifeStylesDbContext context,
        IRequestClient<PatientProfileExistenceRequest> client)
    {
        _context = context;
        _client = client;
    }

    public async Task<UpdatePatientPhysicalActivitiesV2Result> Handle(
        UpdatePatientPhysicalActivitiesV2Command request, CancellationToken cancellationToken)
    {
        var response =
            await _client.GetResponse<PatientProfileExistenceResponse>(
                new PatientProfileExistenceRequest(request.PatientProfileId), cancellationToken);

        if (!response.Message.IsExist) throw new LifeStylesNotFoundException("Profile người dùng", request.PatientProfileId);

        var existingActivities = _context.PatientPhysicalActivities
            .Where(x => x.PatientProfileId == request.PatientProfileId);

        _context.PatientPhysicalActivities.RemoveRange(existingActivities);

        var newActivities = request.Activities
            .Select(activity => new Models.PatientPhysicalActivity
            {
                PatientProfileId = request.PatientProfileId,
                PhysicalActivityId = activity.PhysicalActivityId,
                PreferenceLevel = activity.PreferenceLevel
            });

        await _context.PatientPhysicalActivities.AddRangeAsync(newActivities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new UpdatePatientPhysicalActivitiesV2Result(true);
    }
}