using Microsoft.AspNetCore.Mvc;
using Profile.API.Common.Helpers;
using Profile.API.Domains.Public.PatientProfiles.Enum;

namespace Profile.API.Domains.Public.PatientProfiles.Features.UpdateMedicalRecord;

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
        app.MapPut("/v1/patients/medical-record", async ([FromBody] UpdateMedicalRecordRequest request,
            ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientProfileId, httpContext.User))
                    throw new ForbiddenException();

                var command = request.Adapt<UpdateMedicalRecordCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<UpdateMedicalRecordResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("UpdateMedicalRecord")
            .WithTags("MedicalRecords")
            .Produces<UpdateMedicalRecordResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Updates the medical record for the specified patient profile. Requires 'User' or 'Admin' role. Returns success status.")
            .WithSummary("Update patient medical record");
    }
}