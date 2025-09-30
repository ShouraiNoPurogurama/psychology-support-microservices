using Microsoft.AspNetCore.Mvc;
using Profile.API.Common.Helpers;
using Profile.API.Domains.Public.PatientProfiles.Enum;

namespace Profile.API.Domains.Public.PatientProfiles.Features.AddMedicalRecord;

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
        app.MapPost("/v1/patients/medical-record", async ([FromBody] AddMedicalRecordRequest request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientProfileId, httpContext.User))
                    throw new ForbiddenException();

                var command = request.Adapt<AddMedicalRecordCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<AddMedicalRecordResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("AddMedicalRecord")
            .WithTags("MedicalRecords")
            .Produces<AddMedicalRecordResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Adds a new medical record for the specified patient profile. Requires 'User' or 'Admin' role. Returns success status.")
            .WithSummary("Add medical record");
    }
}