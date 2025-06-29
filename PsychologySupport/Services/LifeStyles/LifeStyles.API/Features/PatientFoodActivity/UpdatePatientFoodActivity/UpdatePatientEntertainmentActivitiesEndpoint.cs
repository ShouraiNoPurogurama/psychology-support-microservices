using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Features.PatientFoodActivity.CreatePatientFoodActivity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientFoodActivity.UpdatePatientFoodActivity;

public record UpdatePatientFoodActivitiesRequest(Guid PatientProfileId, List<FoodPreference> Activities);

public record UpdatePatientFoodActivitiesResponse(Guid PatientProfileId, List<FoodPreference> Activities);

public class UpdatePatientFoodActivitiesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("patient-Food-activities",
                async (HttpContext httpContext,[FromBody] UpdatePatientFoodActivitiesRequest request, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                        return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );

                    var command = new UpdatePatientFoodActivitiesCommand(
                        request.PatientProfileId,
                        request.Activities.Select(a => (a.FoodActivityId, a.PreferenceLevel)).ToList()
                    );

                    await sender.Send(command);

                    var response = new UpdatePatientFoodActivitiesResponse(
                        request.PatientProfileId,
                        request.Activities
                    );

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("UpdatePatientFoodActivities")
            .WithTags("PatientFoodActivities")
            .Produces<UpdatePatientFoodActivitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update Patient Food Activities")
            .WithSummary("Update Patient Food Activities");
    }
}