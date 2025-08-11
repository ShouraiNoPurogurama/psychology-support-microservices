using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Features.PatientFoodActivity.CreatePatientFoodActivity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features02.PatientFoodActivity.UpdatePatientFoodActivity;

public record UpdatePatientFoodActivitiesV2Request(Guid PatientProfileId, List<FoodPreference> Activities);

public record UpdatePatientFoodActivitiesV2Response(Guid PatientProfileId, List<FoodPreference> Activities);

public class UpdatePatientFoodActivitiesV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v2/me/foodActivities",
                async (HttpContext httpContext, [FromBody] UpdatePatientFoodActivitiesV2Request request, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                        throw new ForbiddenException();

                    var command = new UpdatePatientFoodActivitiesCommand(
                        request.PatientProfileId,
                        request.Activities.Select(a => (a.FoodActivityId, a.PreferenceLevel)).ToList()
                    );

                    await sender.Send(command);

                    var response = new UpdatePatientFoodActivitiesV2Response(
                        request.PatientProfileId,
                        request.Activities
                    );

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("UpdatePatientFoodActivities v2")
            .WithTags("PatientFoodActivities Version 2")
            .Produces<UpdatePatientFoodActivitiesV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update Patient Food Activities")
            .WithSummary("Update Patient Food Activities");
    }
}