using Carter;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Profile.API.PatientProfiles.Features.UpdateMedicalHistory;

public record UpdateMedicalHistoryRequest(
    Guid PatientProfileId,
    string Description,
    DateTimeOffset DiagnosedAt,
    List<Guid> DisorderIds,
    List<Guid> PhysicalSymptomIds);

public record UpdateMedicalHistoryResponse(bool IsSuccess);

public class UpdateMedicalHistoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/patients/medical-history", async ([FromBody] UpdateMedicalHistoryRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateMedicalHistoryCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<UpdateMedicalHistoryResponse>();

                return response.IsSuccess ? Results.Ok(response) : Results.Problem();
            })
            .WithName("UpdateMedicalHistory")
            .Produces<UpdateMedicalHistoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Update Medical History")
            .WithSummary("Update Medical History");
    }
}