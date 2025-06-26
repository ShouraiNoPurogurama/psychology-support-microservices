using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Features.PatientPhysicalActivity.CreatePatientPhysicalActivity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientPhysicalActivity.UpdatePatientPhysicalActivities;

public record UpdatePatientPhysicalActivitiesRequest(Guid PatientProfileId, List<PhysicalPreference> Activities);

public record UpdatePatientPhysicalActivitiesResponse(Guid PatientProfileId, List<PhysicalPreference> Activities);

public class UpdatePatientPhysicalActivitiesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("patient-Physical-activities",
                async (HttpContext httpContext, [FromBody] UpdatePatientPhysicalActivitiesRequest request, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                        return Results.Forbid();

                    var command = new UpdatePatientPhysicalActivitiesCommand(
                        request.PatientProfileId,
                        request.Activities.Select(a => (a.PhysicalActivityId, a.PreferenceLevel)).ToList()
                    );

                    await sender.Send(command);

                    var response = new UpdatePatientPhysicalActivitiesResponse(
                        request.PatientProfileId,
                        request.Activities
                    );

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("UpdatePatientPhysicalActivities")
            .WithTags("PatientPhysicalActivities")
            .Produces<UpdatePatientPhysicalActivitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update Patient Physical Activities")
            .WithSummary("Update Patient Physical Activities");
    }
}