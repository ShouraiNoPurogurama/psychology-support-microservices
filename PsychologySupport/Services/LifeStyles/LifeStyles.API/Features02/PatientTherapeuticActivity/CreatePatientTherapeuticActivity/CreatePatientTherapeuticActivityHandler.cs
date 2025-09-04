using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features02.PatientTherapeuticActivity.CreatePatientTherapeuticActivity;

public record CreatePatientTherapeuticActivityCommand(
    Guid PatientProfileId,
    List<(Guid TherapeuticActivityId, PreferenceLevel PreferenceLevel)> Activities
) : ICommand<CreatePatientTherapeuticActivityResult>;

public record CreatePatientTherapeuticActivityResult(bool IsSucceeded);

public class CreatePatientTherapeuticActivityHandler(
    LifeStylesDbContext context,
    IRequestClient<PatientProfileExistenceRequest> client)
    : ICommandHandler<CreatePatientTherapeuticActivityCommand,
        CreatePatientTherapeuticActivityResult>
{
    public async Task<CreatePatientTherapeuticActivityResult> Handle(CreatePatientTherapeuticActivityCommand request,
        CancellationToken cancellationToken)
    {
        var response =
            await client.GetResponse<PatientProfileExistenceResponse>(
                new PatientProfileExistenceRequest(request.PatientProfileId), cancellationToken);

        if (!response.Message.IsExist) throw new LifeStylesNotFoundException("Profile người dùng", request.PatientProfileId);

        var activities = request.Activities.Select(a => new Models.PatientTherapeuticActivity
            {
                PatientProfileId = request.PatientProfileId,
                TherapeuticActivityId = a.TherapeuticActivityId,
                PreferenceLevel = a.PreferenceLevel
            })
            .ToList();

        context.PatientTherapeuticActivities.AddRange(activities);
        await context.SaveChangesAsync(cancellationToken);

        return new CreatePatientTherapeuticActivityResult(true);
    }
}