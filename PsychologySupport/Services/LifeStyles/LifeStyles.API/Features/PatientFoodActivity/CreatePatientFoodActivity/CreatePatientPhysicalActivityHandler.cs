using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features.PatientFoodActivity.CreatePatientFoodActivity;

public record CreatePatientFoodActivityCommand(
    Guid PatientProfileId,
    List<(Guid FoodActivityId, PreferenceLevel PreferenceLevel)> Activities
) : ICommand<CreatePatientFoodActivityResult>;

public record CreatePatientFoodActivityResult(bool IsSucceeded);

public class CreatePatientFoodActivityHandler(
    LifeStylesDbContext context,
    IRequestClient<PatientProfileExistenceRequest> client)
    : ICommandHandler<CreatePatientFoodActivityCommand,
        CreatePatientFoodActivityResult>
{
    public async Task<CreatePatientFoodActivityResult> Handle(CreatePatientFoodActivityCommand request,
        CancellationToken cancellationToken)
    {
        var response =
            await client.GetResponse<PatientProfileExistenceResponse>(
                new PatientProfileExistenceRequest(request.PatientProfileId), cancellationToken);

        if (!response.Message.IsExist) throw new LifeStylesNotFoundException("Profile người dùng", request.PatientProfileId);

        var activities = request.Activities.Select(a => new Models.PatientFoodActivity
            {
                PatientProfileId = request.PatientProfileId,
                FoodActivityId = a.FoodActivityId,
                PreferenceLevel = a.PreferenceLevel
            })
            .ToList();

        context.PatientFoodActivities.AddRange(activities);
        await context.SaveChangesAsync(cancellationToken);

        return new CreatePatientFoodActivityResult(true);
    }
}