using Carter;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Common.Helpers;
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
        app.MapPost("/patients/medical-record", async ([FromBody] AddMedicalRecordRequest request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientProfileId, httpContext.User))
                    return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );

                var command = request.Adapt<AddMedicalRecordCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<AddMedicalRecordResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("AddMedicalRecord")
            .WithTags("PatientProfiles")
            .Produces<AddMedicalRecordResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Add Medical Record")
            .WithSummary("Add Medical Record");
    }
}