using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features02.PatientEntertainmentActivity.GetPatientEntertainmentActivity;

public record GetPatientEntertainmentActivityV2Request(Guid PatientProfileId);

public record GetPatientEntertainmentActivityV2Response(
    IEnumerable<Models.PatientEntertainmentActivity> PatientEntertainmentActivities
);

public class GetPatientEntertainmentActivityV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v2/me/activities/entertainments",
            async (HttpContext httpContext, Guid patientProfileId, ISender sender) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasAccessToPatientProfile(patientProfileId, httpContext.User))
                    throw new ForbiddenException();

                var query = new GetPatientEntertainmentActivityV2Query(patientProfileId);
                var result = await sender.Send(query);
                var response = result.Adapt<GetPatientEntertainmentActivityV2Response>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetPatientEntertainmentActivities v2")
            .WithTags("PatientEntertainmentActivities Version 2")
            .Produces<GetPatientEntertainmentActivityV2Response>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Patient Entertainment Activities v2")
            .WithSummary("Get Patient Entertainment Activities v2");
    }
}