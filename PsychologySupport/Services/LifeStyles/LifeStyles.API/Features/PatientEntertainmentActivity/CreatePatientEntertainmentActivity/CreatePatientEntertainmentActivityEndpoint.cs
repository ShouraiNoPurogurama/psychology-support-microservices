using BuildingBlocks.Enums;
using Carter;
using LifeStyles.API.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientEntertainmentActivity.CreatePatientEntertainmentActivity;

public record CreatePatientEntertainmentActivityRequest(Guid PatientProfileId, List<EntertainmentPreference> Activities);

public record EntertainmentPreference(Guid EntertainmentActivityId, PreferenceLevel PreferenceLevel);

public record CreatePatientEntertainmentActivityResponse(Guid PatientProfileId, List<EntertainmentPreference> Activities);

public class CreatePatientEntertainmentActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("patient-entertainment-activities",
                async (HttpContext httpContext, [FromBody] CreatePatientEntertainmentActivityRequest request, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                        return Results.Forbid();

                    var command = new CreatePatientEntertainmentActivityCommand(
                        request.PatientProfileId,
                        request.Activities.Select(a => (a.EntertainmentActivityId, a.PreferenceLevel)).ToList()
                    );

                    await sender.Send(command);

                    var response = new CreatePatientEntertainmentActivityResponse(
                        request.PatientProfileId,
                        request.Activities
                    );

                    return Results.Created($"/patient-entertainment-activities/{request.PatientProfileId}", response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreatePatientEntertainmentActivities")
            .WithTags("PatientEntertainmentActivities")
            .Produces<CreatePatientEntertainmentActivityResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .WithDescription("Create Patient Entertainment Activities")
            .WithSummary("Create Patient Entertainment Activities");
    }
}
