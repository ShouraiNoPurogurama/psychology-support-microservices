using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features02.PatientTherapeuticActivity.CreatePatientTherapeuticActivity;

public record CreatePatientTherapeuticActivityV2Request(
    Guid PatientProfileId,
    List<TherapeuticPreferenceV2> Activities
);

public record TherapeuticPreferenceV2(Guid TherapeuticActivityId, PreferenceLevel PreferenceLevel);

public record CreatePatientTherapeuticActivityV2Response(
    Guid PatientProfileId,
    List<TherapeuticPreferenceV2> Activities
);

public class CreatePatientTherapeuticActivityV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v2/me/activities/therapeutic", async (HttpContext httpContext,
                [FromBody] CreatePatientTherapeuticActivityV2Request request, ISender sender) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                    throw new ForbiddenException();

                var command = new CreatePatientTherapeuticActivityCommand(
                    request.PatientProfileId,
                    request.Activities.Select(a => (a.TherapeuticActivityId, a.PreferenceLevel)).ToList()
                );

                await sender.Send(command);

                var response = new CreatePatientTherapeuticActivityV2Response(
                    request.PatientProfileId,
                    request.Activities
                );

                return Results.Created($"/v2/me/{request.PatientProfileId}/therapeuticActivities", response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreatePatientTherapeuticActivity v2")
            .WithTags("PatientTherapeuticActivities Version 2")
            .Produces<CreatePatientTherapeuticActivityV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create multiple Patient Therapeutic Activities")
            .WithSummary("Create multiple Patient Therapeutic Activities");
    }
}