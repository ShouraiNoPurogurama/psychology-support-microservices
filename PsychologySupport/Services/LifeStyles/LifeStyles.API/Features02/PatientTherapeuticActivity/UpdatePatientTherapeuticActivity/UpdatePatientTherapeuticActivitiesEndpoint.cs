using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Features.PatientTherapeuticActivity.CreatePatientTherapeuticActivity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features02.PatientTherapeuticActivity.UpdatePatientTherapeuticActivity;

public record UpdatePatientTherapeuticActivitiesV2Request(Guid PatientProfileId, List<TherapeuticPreference> Activities);

public record UpdatePatientTherapeuticActivitiesV2Response(Guid PatientProfileId, List<TherapeuticPreference> Activities);

public class UpdatePatientTherapeuticActivitiesV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v2/me/therapeuticActivities",
                async (HttpContext httpContext, [FromBody] UpdatePatientTherapeuticActivitiesV2Request request, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                        throw new ForbiddenException();

                    var command = new UpdatePatientTherapeuticActivitiesCommand(
                        request.PatientProfileId,
                        request.Activities.Select(a => (a.TherapeuticActivityId, a.PreferenceLevel)).ToList()
                    );

                    await sender.Send(command);

                    var response = new UpdatePatientTherapeuticActivitiesV2Response(
                        request.PatientProfileId,
                        request.Activities
                    );

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("UpdatePatientTherapeuticActivities v2")
            .WithTags("PatientTherapeuticActivities Version 2")
            .Produces<UpdatePatientTherapeuticActivitiesV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update Patient Therapeutic Activities")
            .WithSummary("Update Patient Therapeutic Activities");
    }
}