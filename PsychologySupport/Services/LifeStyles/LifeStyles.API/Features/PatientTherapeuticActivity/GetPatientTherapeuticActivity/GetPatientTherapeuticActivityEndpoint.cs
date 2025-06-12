using Carter;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.PatientTherapeuticActivity.GetPatientTherapeuticActivity;

public record GetPatientTherapeuticActivityRequest(Guid PatientProfileId);

public record GetPatientTherapeuticActivityResponse(IEnumerable<Models.PatientTherapeuticActivity> PatientTherapeuticActivities);

public class GetPatientTherapeuticActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patient-therapeutic-activities/{patientProfileId:guid}",
                async (Guid patientProfileId, ISender sender) =>
                {
                    var query = new GetPatientTherapeuticActivityQuery(patientProfileId);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetPatientTherapeuticActivityResponse>();

                    return Results.Ok(response);
                })
            .WithName("GetPatientTherapeuticActivities")
            .WithTags("PatientTherapeuticActivities")
            .Produces<GetPatientTherapeuticActivityResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Patient Therapeutic Activities")
            .WithSummary("Get Patient Therapeutic Activities");
    }
}