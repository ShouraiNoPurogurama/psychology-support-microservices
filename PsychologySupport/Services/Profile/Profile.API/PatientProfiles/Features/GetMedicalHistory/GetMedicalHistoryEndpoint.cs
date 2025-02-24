using Carter;
using MediatR;
using Mapster;
using Profile.API.PatientProfiles.Dtos;



public record GetMedicalHistoryRequest(Guid PatientId);

public record GetMedicalHistoryResponse(MedicalHistoryDto History);

namespace Profile.API.PatientProfiles.Features.GetMedicalHistory
{
    public class GetMedicalHistoryEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/patients/{patientId:guid}/medical-history", async (Guid patientId, ISender sender) =>
            {
                var query = new GetMedicalHistoryQuery(patientId);
                var result = await sender.Send(query);
                var response = result.Adapt<GetMedicalHistoryResponse>();
                return Results.Ok(response);
            })
                .WithName("GetMedicalHistoryByPatientId")
                .Produces<GetMedicalHistoryResponse>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("Get MedicalHistory By PatientId")
                .WithSummary("Get MedicalHistory By PatientId");
        }
    }
}
