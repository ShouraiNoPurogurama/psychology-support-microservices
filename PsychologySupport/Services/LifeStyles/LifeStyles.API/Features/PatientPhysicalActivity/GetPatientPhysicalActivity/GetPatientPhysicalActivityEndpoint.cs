using Carter;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.PatientPhysicalActivity.GetPatientPhysicalActivity;

public record GetPatientPhysicalActivityRequest(Guid PatientProfileId);

public record GetPatientPhysicalActivityResponse(IEnumerable<Models.PatientPhysicalActivity> PatientPhysicalActivities);

public class GetPatientPhysicalActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patient-physical-activities/{patientProfileId:guid}",
                async (Guid patientProfileId, ISender sender) =>
                {
                    var query = new GetPatientPhysicalActivityQuery(patientProfileId);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetPatientPhysicalActivityResponse>();

                    return Results.Ok(response);
                })
            .WithName("GetPatientPhysicalActivities")
            .WithTags("PatientPhysicalActivities")
            .Produces<GetPatientPhysicalActivityResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Patient Physical Activities")
            .WithSummary("Get Patient Physical Activities");
    }
}