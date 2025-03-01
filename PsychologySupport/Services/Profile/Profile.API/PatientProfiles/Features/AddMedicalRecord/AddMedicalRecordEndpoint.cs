using Carter;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Profile.API.PatientProfiles.Enum;

namespace Profile.API.PatientProfiles.Features.AddMedicalRecord;

public record AddMedicalRecordRequest(
    Guid PatientProfileId,
    Guid DoctorId,
    string Notes,
    MedicalRecordStatus Status,
    List<Guid> ExistingDisorderIds);

public record AddMedicalRecordResponse(bool IsSuccess);

public class AddMedicalRecordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/patients/medical-record", async ([FromBody] AddMedicalRecordRequest request, ISender sender) =>
            {
                var command = request.Adapt<AddMedicalRecordCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<AddMedicalRecordResponse>();

                return Results.Ok(response);
            })
            .WithName("AddMedicalRecord")
            .Produces<AddMedicalRecordResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Add Medical Record")
            .WithSummary("Add Medical Record");
    }
}