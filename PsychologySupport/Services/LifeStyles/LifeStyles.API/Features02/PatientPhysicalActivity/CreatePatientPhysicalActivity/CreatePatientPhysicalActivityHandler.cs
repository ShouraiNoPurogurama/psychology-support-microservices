using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features02.PatientPhysicalActivity.CreatePatientPhysicalActivity;

public record CreatePatientPhysicalActivityV2Command(
    Guid PatientProfileId,
    List<(Guid PhysicalActivityId, PreferenceLevel PreferenceLevel)> Activities
) : ICommand<CreatePatientPhysicalActivityV2Result>;

public record CreatePatientPhysicalActivityV2Result(bool IsSucceeded);

public class CreatePatientPhysicalActivityV2Handler(
    LifeStylesDbContext context,
    IRequestClient<PatientProfileExistenceRequest> client
) : ICommandHandler<CreatePatientPhysicalActivityV2Command, CreatePatientPhysicalActivityV2Result>
{
    public async Task<CreatePatientPhysicalActivityV2Result> Handle(
        CreatePatientPhysicalActivityV2Command request,
        CancellationToken cancellationToken)
    {
        var response = await client.GetResponse<PatientProfileExistenceResponse>(
            new PatientProfileExistenceRequest(request.PatientProfileId), cancellationToken);

        if (!response.Message.IsExist)
            throw new LifeStylesNotFoundException("Profile người dùng", request.PatientProfileId);

        var activities = request.Activities.Select(a => new Models.PatientPhysicalActivity
        {
            PatientProfileId = request.PatientProfileId,
            PhysicalActivityId = a.PhysicalActivityId,
            PreferenceLevel = a.PreferenceLevel
        }).ToList();

        context.PatientPhysicalActivities.AddRange(activities);
        await context.SaveChangesAsync(cancellationToken);

        return new CreatePatientPhysicalActivityV2Result(true);
    }
}