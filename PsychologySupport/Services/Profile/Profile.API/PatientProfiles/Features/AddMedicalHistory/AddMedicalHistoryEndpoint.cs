using Carter;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Common.Helpers;
using Profile.API.PatientProfiles.Features.AddMedicalRecord;

namespace Profile.API.PatientProfiles.Features.AddMedicalHistory;

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
                    return Results.Problem(
                              statusCode: StatusCodes.Status403Forbidden,
                              title: "Forbidden",
                              detail: "You do not have permission to access this resource."
                          );

                var command = request.Adapt<AddMedicalHistoryCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<AddMedicalRecordResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("AddMedicalHistory")
            .WithTags("PatientProfiles")
            .Produces<AddMedicalRecordResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Add Medical History")
            .WithSummary("Add Medical History");
    }
}