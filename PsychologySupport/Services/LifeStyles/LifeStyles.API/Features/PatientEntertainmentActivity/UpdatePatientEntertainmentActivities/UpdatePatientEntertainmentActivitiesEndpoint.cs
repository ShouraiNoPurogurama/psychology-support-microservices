using BuildingBlocks.Enums;
using Carter;
using LifeStyles.API.Features.PatientEntertainmentActivity.CreatePatientEntertainmentActivity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientEntertainmentActivity.UpdatePatientEntertainmentActivities;

public record UpdatePatientEntertainmentActivitiesRequest(Guid PatientProfileId, List<EntertainmentPreference> Activities);

public record UpdatePatientEntertainmentActivitiesResponse(Guid PatientProfileId, List<EntertainmentPreference> Activities);

public class UpdatePatientEntertainmentActivitiesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("patient-entertainment-activities",
                async ([FromBody] UpdatePatientEntertainmentActivitiesRequest request, ISender sender) =>
                {
                    var command = new UpdatePatientEntertainmentActivitiesCommand(
                        request.PatientProfileId,
                        request.Activities.Select(a => (a.EntertainmentActivityId, a.PreferenceLevel)).ToList()
                    );

                    await sender.Send(command);

                    var response = new UpdatePatientEntertainmentActivitiesResponse(
                        request.PatientProfileId,
                        request.Activities
                    );

                    return Results.Ok(response);
                })
            .WithName("UpdatePatientEntertainmentActivities")
            .WithTags("PatientEntertainmentActivities")
            .Produces<UpdatePatientEntertainmentActivitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update Patient Entertainment Activities")
            .WithSummary("Update Patient Entertainment Activities");
    }
}