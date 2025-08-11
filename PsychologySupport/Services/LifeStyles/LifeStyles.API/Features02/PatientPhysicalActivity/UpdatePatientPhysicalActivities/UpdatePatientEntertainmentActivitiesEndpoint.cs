using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Features.PatientPhysicalActivity.CreatePatientPhysicalActivity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features02.PatientPhysicalActivity.UpdatePatientPhysicalActivities;

public record UpdatePatientPhysicalActivitiesV2Request(
        Guid PatientProfileId,
        List<PhysicalPreference> Activities
    );

public record UpdatePatientPhysicalActivitiesV2Response(
    Guid PatientProfileId,
    List<PhysicalPreference> Activities
);
public class UpdatePatientPhysicalActivitiesV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v2/me/physicalActivities",
                async (HttpContext httpContext, [FromBody] UpdatePatientPhysicalActivitiesV2Request request, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                        return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );

                    var command = new UpdatePatientPhysicalActivitiesV2Command(
                        request.PatientProfileId,
                        request.Activities.Select(a => (a.PhysicalActivityId, a.PreferenceLevel)).ToList()
                    );

                    await sender.Send(command);

                    var response = new UpdatePatientPhysicalActivitiesV2Response(
                        request.PatientProfileId,
                        request.Activities
                    );

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("UpdatePatientPhysicalActivities v2")
            .WithTags("PatientPhysicalActivities Version 2")
            .Produces<UpdatePatientPhysicalActivitiesV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update Patient Physical Activities")
            .WithSummary("Update Patient Physical Activities");
    }
}