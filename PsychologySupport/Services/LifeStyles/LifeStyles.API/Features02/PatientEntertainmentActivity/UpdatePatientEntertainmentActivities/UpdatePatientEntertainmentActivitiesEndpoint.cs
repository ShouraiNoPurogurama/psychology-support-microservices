using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Features.PatientEntertainmentActivity.CreatePatientEntertainmentActivity;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features02.PatientEntertainmentActivity.UpdatePatientEntertainmentActivities;

public record UpdatePatientEntertainmentActivitiesV2Request(
    Guid PatientProfileId,
    List<EntertainmentPreference> Activities
);

public record UpdatePatientEntertainmentActivitiesV2Response(
    Guid PatientProfileId,
    List<EntertainmentPreference> Activities
);

public class UpdatePatientEntertainmentActivitiesV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v2/me/entertainmentActivities",
            async (HttpContext httpContext, [FromBody] UpdatePatientEntertainmentActivitiesV2Request request, ISender sender) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                    throw new ForbiddenException();

                var command = request.Adapt<UpdatePatientEntertainmentActivitiesV2Command>();

                await sender.Send(command);

                var response = request.Adapt<UpdatePatientEntertainmentActivitiesV2Response>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("UpdatePatientEntertainmentActivities v2")
            .WithTags("PatientEntertainmentActivities Version 2")
            .Produces<UpdatePatientEntertainmentActivitiesV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update Patient Entertainment Activities v2")
            .WithSummary("Update Patient Entertainment Activities v2");
    }
}