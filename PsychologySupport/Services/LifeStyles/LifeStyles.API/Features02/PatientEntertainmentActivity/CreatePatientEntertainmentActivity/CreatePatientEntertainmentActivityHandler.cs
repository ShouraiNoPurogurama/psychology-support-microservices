using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features02.PatientEntertainmentActivity.CreatePatientEntertainmentActivity;

public record CreatePatientEntertainmentActivityV2Command(
    Guid PatientProfileId,
    List<(Guid EntertainmentActivityId, PreferenceLevel PreferenceLevel)> Activities
) : ICommand<CreatePatientEntertainmentActivityV2Result>;

public record CreatePatientEntertainmentActivityV2Result(bool IsSucceeded);

// Handler
public class CreatePatientEntertainmentActivityV2Handler(
    LifeStylesDbContext context,
    IRequestClient<PatientProfileExistenceRequest> client
) : ICommandHandler<CreatePatientEntertainmentActivityV2Command, CreatePatientEntertainmentActivityV2Result>
{
    public async Task<CreatePatientEntertainmentActivityV2Result> Handle(
        CreatePatientEntertainmentActivityV2Command request,
        CancellationToken cancellationToken)
    {
        var response = await client.GetResponse<PatientProfileExistenceResponse>(
            new PatientProfileExistenceRequest(request.PatientProfileId),
            cancellationToken
        );

        if (!response.Message.IsExist)
            throw new LifeStylesNotFoundException("PatientProfile", request.PatientProfileId);

        var activities = request.Activities.Select(a => new Models.PatientEntertainmentActivity
        {
            PatientProfileId = request.PatientProfileId,
            EntertainmentActivityId = a.EntertainmentActivityId,
            PreferenceLevel = a.PreferenceLevel
        }).ToList();

        context.PatientEntertainmentActivities.AddRange(activities);
        await context.SaveChangesAsync(cancellationToken);

        return new CreatePatientEntertainmentActivityV2Result(true);
    }
}