using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features02.PatientEntertainmentActivity.UpdatePatientEntertainmentActivities;

public record UpdatePatientEntertainmentActivitiesV2Command(
    Guid PatientProfileId,
    List<(Guid EntertainmentActivityId, PreferenceLevel PreferenceLevel)> Activities
) : ICommand<UpdatePatientEntertainmentActivitiesV2Result>;

public record UpdatePatientEntertainmentActivitiesV2Result(bool IsSucceeded);

public class UpdatePatientEntertainmentActivitiesV2Handler(
    LifeStylesDbContext context,
    IRequestClient<PatientProfileExistenceRequest> client
) : ICommandHandler<UpdatePatientEntertainmentActivitiesV2Command, UpdatePatientEntertainmentActivitiesV2Result>
{
    public async Task<UpdatePatientEntertainmentActivitiesV2Result> Handle(
        UpdatePatientEntertainmentActivitiesV2Command request,
        CancellationToken cancellationToken)
    {
        var response = await client.GetResponse<PatientProfileExistenceResponse>(
            new PatientProfileExistenceRequest(request.PatientProfileId),
            cancellationToken
        );

        if (!response.Message.IsExist)
            throw new LifeStylesNotFoundException("PatientProfile", request.PatientProfileId);

        var existingActivities = context.PatientEntertainmentActivities
            .Where(x => x.PatientProfileId == request.PatientProfileId);

        context.PatientEntertainmentActivities.RemoveRange(existingActivities);

        var newActivities = request.Activities
            .Select(activity => new Models.PatientEntertainmentActivity
            {
                PatientProfileId = request.PatientProfileId,
                EntertainmentActivityId = activity.EntertainmentActivityId,
                PreferenceLevel = activity.PreferenceLevel
            });

        await context.PatientEntertainmentActivities.AddRangeAsync(newActivities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new UpdatePatientEntertainmentActivitiesV2Result(true);
    }
}