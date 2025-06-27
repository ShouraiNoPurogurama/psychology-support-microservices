using Carter;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Common.Helpers;
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
        app.MapPut("/patients/medical-record", async ([FromBody] UpdateMedicalRecordRequest request,
            ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientProfileId, httpContext.User))
                    return Results.Problem(
                              statusCode: StatusCodes.Status403Forbidden,
                              title: "Forbidden",
                              detail: "You do not have permission to access this resource."
                          );

                var command = request.Adapt<UpdateMedicalRecordCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<UpdateMedicalRecordResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("UpdateMedicalRecord")
            .WithTags("PatientProfiles")
            .Produces<UpdateMedicalRecordResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Update Medical Record")
            .WithSummary("Update Medical Record");
    }
}