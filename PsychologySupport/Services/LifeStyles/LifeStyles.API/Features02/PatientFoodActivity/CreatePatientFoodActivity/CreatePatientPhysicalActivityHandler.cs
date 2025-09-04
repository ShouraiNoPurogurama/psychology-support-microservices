using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features02.PatientFoodActivity.CreatePatientFoodActivity;

public record CreatePatientFoodActivityV2Command(
    Guid PatientProfileId,
    List<(Guid FoodActivityId, PreferenceLevel PreferenceLevel)> Activities
) : ICommand<CreatePatientFoodActivityV2Result>;

public record CreatePatientFoodActivityV2Result(bool IsSucceeded);

public class CreatePatientFoodActivityV2Handler(
    LifeStylesDbContext context,
    IRequestClient<PatientProfileExistenceRequest> client)
    : ICommandHandler<CreatePatientFoodActivityV2Command,
        CreatePatientFoodActivityV2Result>
{
    public async Task<CreatePatientFoodActivityV2Result> Handle(CreatePatientFoodActivityV2Command request,
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

        return new CreatePatientFoodActivityV2Result(true);
    }
}