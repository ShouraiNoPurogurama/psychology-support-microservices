using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features02.PatientEntertainmentActivity.CreatePatientEntertainmentActivity;

public record CreatePatientEntertainmentActivityV2Request(
    Guid PatientProfileId,
    List<EntertainmentPreferenceV2> Activities
);

public record EntertainmentPreferenceV2(Guid EntertainmentActivityId, PreferenceLevel PreferenceLevel);

public record CreatePatientEntertainmentActivityV2Response(
    Guid PatientProfileId,
    List<EntertainmentPreferenceV2> Activities
);

public class CreatePatientEntertainmentActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v2/me/entertainmentActivities",
                async (HttpContext httpContext, [FromBody] CreatePatientEntertainmentActivityV2Request request, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                        throw new ForbiddenException();

                    var command = new CreatePatientEntertainmentActivityV2Command(
                        request.PatientProfileId,
                        request.Activities.Select(a => (a.EntertainmentActivityId, a.PreferenceLevel)).ToList()
                    );

                    await sender.Send(command);

                    var response = new CreatePatientEntertainmentActivityV2Response(
                        request.PatientProfileId,
                        request.Activities
                    );

                    return Results.Created($"/v2/me/entertainmentActivities/{request.PatientProfileId}", response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreatePatientEntertainmentActivities v2")
            .WithTags("PatientEntertainmentActivities Version 2")
            .Produces<CreatePatientEntertainmentActivityV2Response>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .WithDescription("Create Patient Entertainment Activities v2")
            .WithSummary("Create Patient Entertainment Activities v2");
    }
}
