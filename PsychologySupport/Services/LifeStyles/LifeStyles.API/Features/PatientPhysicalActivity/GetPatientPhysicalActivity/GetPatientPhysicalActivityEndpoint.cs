using Carter;
using LifeStyles.API.Common;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features.PatientPhysicalActivity.GetPatientPhysicalActivity;

public record GetPatientPhysicalActivityRequest(Guid PatientProfileId);

public record GetPatientPhysicalActivityResponse(IEnumerable<Models.PatientPhysicalActivity> PatientPhysicalActivities);

public class GetPatientPhysicalActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patient-physical-activities/{patientProfileId:guid}",
                async (HttpContext httpContext, Guid patientProfileId, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(patientProfileId, httpContext.User))
                        return Results.Forbid();

                    var query = new GetPatientPhysicalActivityQuery(patientProfileId);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetPatientPhysicalActivityResponse>();

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetPatientPhysicalActivities")
            .WithTags("PatientPhysicalActivities")
            .Produces<GetPatientPhysicalActivityResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Patient Physical Activities")
            .WithSummary("Get Patient Physical Activities");
    }
}