using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features.PatientPhysicalActivity.CreatePatientPhysicalActivity;

public record CreatePatientPhysicalActivityCommand(
    Guid PatientProfileId,
    List<(Guid PhysicalActivityId, PreferenceLevel PreferenceLevel)> Activities
) : ICommand<CreatePatientPhysicalActivityResult>;

public record CreatePatientPhysicalActivityResult(bool IsSucceeded);

public class CreatePatientPhysicalActivityHandler(
    LifeStylesDbContext context,
    IRequestClient<PatientProfileExistenceRequest> client)
    : ICommandHandler<CreatePatientPhysicalActivityCommand,
        CreatePatientPhysicalActivityResult>
{
    public async Task<CreatePatientPhysicalActivityResult> Handle(CreatePatientPhysicalActivityCommand request,
        CancellationToken cancellationToken)
    {
        var response =
            await client.GetResponse<PatientProfileExistenceResponse>(
                new PatientProfileExistenceRequest(request.PatientProfileId), cancellationToken);

        if (!response.Message.IsExist) throw new LifeStylesNotFoundException("Profile người dùng", request.PatientProfileId);

        var activities = request.Activities.Select(a => new Models.PatientPhysicalActivity
            {
                PatientProfileId = request.PatientProfileId,
                PhysicalActivityId = a.PhysicalActivityId,
                PreferenceLevel = a.PreferenceLevel
            })
            .ToList();

        context.PatientPhysicalActivities.AddRange(activities);
        await context.SaveChangesAsync(cancellationToken);

        return new CreatePatientPhysicalActivityResult(true);
    }
}