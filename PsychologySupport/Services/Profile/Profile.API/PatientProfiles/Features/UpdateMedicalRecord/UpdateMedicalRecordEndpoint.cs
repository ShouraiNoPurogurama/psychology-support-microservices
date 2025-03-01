using Carter;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Profile.API.PatientProfiles.Enum;

namespace Profile.API.PatientProfiles.Features.UpdateMedicalRecord;

public record UpdateMedicalRecordRequest(
    Guid PatientProfileId,
    Guid DoctorId,
    Guid MedicalRecordId,
    string Notes,
    MedicalRecordStatus Status,
    List<Guid> DisorderIds);

public record UpdateMedicalRecordResponse(bool IsSuccess);

public class UpdateMedicalRecordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/patients/medical-record", async ([FromBody] UpdateMedicalRecordRequest request, ISender sender) =>
            {
                if (request == null)
                    return Results.BadRequest("Invalid request payload.");

                var command = request.Adapt<UpdateMedicalRecordCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<UpdateMedicalRecordResponse>();

                return Results.Ok(response);
            })
            .WithName("UpdateMedicalRecord")
            .Produces<UpdateMedicalRecordResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Update Medical Record")
            .WithSummary("Update Medical Record");
    }
}