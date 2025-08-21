using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features.PatientPhysicalActivity.GetPatientPhysicalActivity;

public record GetPatientPhysicalActivityV2Request(Guid PatientProfileId);

public record GetPatientPhysicalActivityV2Response(
    IEnumerable<Models.PatientPhysicalActivity> PatientPhysicalActivities
);

public class GetPatientPhysicalActivityV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v2/me/activities/physicals",
                async (HttpContext httpContext, Guid patientProfileId, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(patientProfileId, httpContext.User))
                        throw new ForbiddenException();

                    var query = new GetPatientPhysicalActivityV2Query(patientProfileId);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetPatientPhysicalActivityV2Response>();

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetPatientPhysicalActivities v2")
            .WithTags("PatientPhysicalActivities Version 2")
            .Produces<GetPatientPhysicalActivityV2Response>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Patient Physical Activities")
            .WithSummary("Get Patient Physical Activities");
    }
}