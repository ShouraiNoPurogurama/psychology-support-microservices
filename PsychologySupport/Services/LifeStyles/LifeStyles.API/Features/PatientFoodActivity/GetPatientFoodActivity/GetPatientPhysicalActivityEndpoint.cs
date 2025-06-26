using Carter;
using LifeStyles.API.Common;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features.PatientFoodActivity.GetPatientFoodActivity;

public record GetPatientFoodActivityRequest(Guid PatientProfileId);

public record GetPatientFoodActivityResponse(IEnumerable<Models.PatientFoodActivity> PatientFoodActivities);

public class GetPatientFoodActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patient-Food-activities/{patientProfileId:guid}",
                async (HttpContext httpContext,Guid patientProfileId, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(patientProfileId, httpContext.User))
                        return Results.Forbid();

                    var query = new GetPatientFoodActivityQuery(patientProfileId);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetPatientFoodActivityResponse>();

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetPatientFoodActivities")
            .WithTags("PatientFoodActivities")
            .Produces<GetPatientFoodActivityResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Patient Food Activities")
            .WithSummary("Get Patient Food Activities");
    }
}