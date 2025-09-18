using Microsoft.AspNetCore.Mvc;
using Profile.API.Common.Helpers;
using Profile.API.Domains.Public.PatientProfiles.Features.AddMedicalRecord;

namespace Profile.API.Domains.Public.PatientProfiles.Features.AddMedicalHistory;

public record AddMedicalHistoryRequest(
    Guid PatientProfileId,
    string Description,
    DateTimeOffset DiagnosedAt,
    List<Guid> SpecificMentalDisorderIds,
    List<Guid> PhysicalSymptomIds);

public record AddMedicalHistoryResponse(bool IsSuccess);

public class AddMedicalHistoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/patients/medical-history", async ([FromBody] AddMedicalHistoryRequest request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientProfileId, httpContext.User))
                    throw new ForbiddenException();

                var command = request.Adapt<AddMedicalHistoryCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<AddMedicalRecordResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("AddMedicalHistory")
            .WithTags("MedicalHistory")
            .Produces<AddMedicalRecordResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Adds a new medical history entry for the specified patient profile. Requires 'User' or 'Admin' role. Returns success status.")
            .WithSummary("Add medical history");
    }
}