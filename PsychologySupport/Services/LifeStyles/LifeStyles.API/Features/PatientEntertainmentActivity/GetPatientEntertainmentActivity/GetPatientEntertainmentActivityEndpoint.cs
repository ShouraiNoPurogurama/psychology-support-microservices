using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features.PatientEntertainmentActivity.GetPatientEntertainmentActivity;

public record GetPatientEntertainmentActivityRequest(Guid PatientProfileId);

public record GetPatientEntertainmentActivityResponse(
    IEnumerable<Models.PatientEntertainmentActivity> PatientEntertainmentActivities);

public class GetPatientEntertainmentActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patient-entertainment-activities/{patientProfileId:guid}",
                async (HttpContext httpContext,Guid patientProfileId, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(patientProfileId, httpContext.User))
                        throw new ForbiddenException();
                    
                    var query = new GetPatientEntertainmentActivityQuery(patientProfileId);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetPatientEntertainmentActivityResponse>();

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetPatientEntertainmentActivities")
            .Produces<GetPatientEntertainmentActivityResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Patient Entertainment Activities")
            .WithSummary("Get Patient Entertainment Activities");
    }
}