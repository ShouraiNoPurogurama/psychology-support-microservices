using Carter;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.PatientEntertainmentActivity.GetPatientEntertainmentActivity;

public record GetPatientEntertainmentActivityRequest(Guid PatientProfileId);

public record GetPatientEntertainmentActivityResponse(
    IEnumerable<Models.PatientEntertainmentActivity> PatientEntertainmentActivities);

public class GetPatientEntertainmentActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patient-entertainment-activities/{patientProfileId:guid}",
                async (Guid patientProfileId, ISender sender) =>
                {
                    var query = new GetPatientEntertainmentActivityQuery(patientProfileId);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetPatientEntertainmentActivityResponse>();

                    return Results.Ok(response);
                })
            .WithName("GetPatientEntertainmentActivities")
            .Produces<GetPatientEntertainmentActivityResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Patient Entertainment Activities")
            .WithSummary("Get Patient Entertainment Activities");
    }
}