using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Features.PatientTherapeuticActivity.CreatePatientTherapeuticActivity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientTherapeuticActivity.UpdatePatientTherapeuticActivity;

public record UpdatePatientTherapeuticActivitiesRequest(Guid PatientProfileId, List<TherapeuticPreference> Activities);

public record UpdatePatientTherapeuticActivitiesResponse(Guid PatientProfileId, List<TherapeuticPreference> Activities);

public class UpdatePatientTherapeuticActivitiesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("patient-therapeutic-activities",
                async (HttpContext httpContext, [FromBody] UpdatePatientTherapeuticActivitiesRequest request, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                        return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );

                    var command = new UpdatePatientTherapeuticActivitiesCommand(
                        request.PatientProfileId,
                        request.Activities.Select(a => (a.TherapeuticActivityId, a.PreferenceLevel)).ToList()
                    );

                    await sender.Send(command);

                    var response = new UpdatePatientTherapeuticActivitiesResponse(
                        request.PatientProfileId,
                        request.Activities
                    );

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("UpdatePatientTherapeuticActivities")
            .WithTags("PatientTherapeuticActivities")
            .Produces<UpdatePatientTherapeuticActivitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update Patient Therapeutic Activities")
            .WithSummary("Update Patient Therapeutic Activities");
    }
}