using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features02.PatientFoodActivity.UpdatePatientFoodActivity;

public record UpdatePatientFoodActivitiesCommand(
    Guid PatientProfileId,
    List<(Guid FoodActivityId, PreferenceLevel PreferenceLevel)> Activities)
    : ICommand<UpdatePatientFoodActivitiesResult>;

public record UpdatePatientFoodActivitiesResult(bool IsSucceeded);

public class UpdatePatientFoodActivitiesHandler
    : ICommandHandler<UpdatePatientFoodActivitiesCommand, UpdatePatientFoodActivitiesResult>
{
    private readonly IRequestClient<PatientProfileExistenceRequest> _client;
    private readonly LifeStylesDbContext _context;

    public UpdatePatientFoodActivitiesHandler(LifeStylesDbContext context,
        IRequestClient<PatientProfileExistenceRequest> client)
    {
        _context = context;
        _client = client;
    }

    public async Task<UpdatePatientFoodActivitiesResult> Handle(
        UpdatePatientFoodActivitiesCommand request, CancellationToken cancellationToken)
    {
        var response =
            await _client.GetResponse<PatientProfileExistenceResponse>(
                new PatientProfileExistenceRequest(request.PatientProfileId), cancellationToken);

        if (!response.Message.IsExist) throw new LifeStylesNotFoundException("Profile người dùng", request.PatientProfileId);

        var existingActivities = _context.PatientFoodActivities
            .Where(x => x.PatientProfileId == request.PatientProfileId);

        _context.PatientFoodActivities.RemoveRange(existingActivities);

        var newActivities = request.Activities
            .Select(activity => new Models.PatientFoodActivity
            {
                PatientProfileId = request.PatientProfileId,
                FoodActivityId = activity.FoodActivityId,
                PreferenceLevel = activity.PreferenceLevel
            });

        await _context.PatientFoodActivities.AddRangeAsync(newActivities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new UpdatePatientFoodActivitiesResult(true);
    }
}