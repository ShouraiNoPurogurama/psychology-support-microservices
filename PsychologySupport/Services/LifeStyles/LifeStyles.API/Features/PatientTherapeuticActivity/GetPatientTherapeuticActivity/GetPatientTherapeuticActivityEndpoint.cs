using Carter;
using LifeStyles.API.Common;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features.PatientTherapeuticActivity.GetPatientTherapeuticActivity;

public record GetPatientTherapeuticActivityRequest(Guid PatientProfileId);

public record GetPatientTherapeuticActivityResponse(IEnumerable<Models.PatientTherapeuticActivity> PatientTherapeuticActivities);

public class GetPatientTherapeuticActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patient-therapeutic-activities/{patientProfileId:guid}",
                async (HttpContext httpContext, Guid patientProfileId, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(patientProfileId, httpContext.User))
                        return Results.Forbid();

                    var query = new GetPatientTherapeuticActivityQuery(patientProfileId);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetPatientTherapeuticActivityResponse>();

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetPatientTherapeuticActivities")
            .WithTags("PatientTherapeuticActivities")
            .Produces<GetPatientTherapeuticActivityResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Patient Therapeutic Activities")
            .WithSummary("Get Patient Therapeutic Activities");
    }
}