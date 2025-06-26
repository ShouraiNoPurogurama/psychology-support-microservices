using BuildingBlocks.Enums;
using Carter;
using LifeStyles.API.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientTherapeuticActivity.CreatePatientTherapeuticActivity;

public record CreatePatientTherapeuticActivityRequest(
    Guid PatientProfileId,
    List<TherapeuticPreference> Activities
);

public record TherapeuticPreference(Guid TherapeuticActivityId, PreferenceLevel PreferenceLevel);

public record CreatePatientTherapeuticActivityResponse(
    Guid PatientProfileId,
    List<TherapeuticPreference> Activities
);

public class CreatePatientTherapeuticActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("patient-therapeutic-activities", async (HttpContext httpContext,
                [FromBody] CreatePatientTherapeuticActivityRequest request, ISender sender) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                    return Results.Forbid();

                var command = new CreatePatientTherapeuticActivityCommand(
                    request.PatientProfileId,
                    request.Activities.Select(a => (a.TherapeuticActivityId, a.PreferenceLevel)).ToList()
                );

                await sender.Send(command);

                var response = new CreatePatientTherapeuticActivityResponse(
                    request.PatientProfileId,
                    request.Activities
                );

                return Results.Created($"/patient-Therapeutic-activities/{request.PatientProfileId}", response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreatePatientTherapeuticActivity")
            .WithTags("PatientTherapeuticActivities")
            .Produces<CreatePatientTherapeuticActivityResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create multiple Patient Therapeutic Activities")
            .WithSummary("Create multiple Patient Therapeutic Activities");
    }
}