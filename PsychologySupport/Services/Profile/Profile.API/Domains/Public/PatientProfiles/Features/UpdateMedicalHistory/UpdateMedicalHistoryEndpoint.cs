using Microsoft.AspNetCore.Mvc;
using Profile.API.Common.Helpers;

namespace Profile.API.Domains.Public.PatientProfiles.Features.UpdateMedicalHistory;

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
        app.MapPut("/patients/medical-history", async ([FromBody] UpdateMedicalHistoryRequest request, 
            ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientProfileId, httpContext.User))
                    throw new ForbiddenException();

                var command = request.Adapt<UpdateMedicalHistoryCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<UpdateMedicalHistoryResponse>();

                return response.IsSuccess ? Results.Ok(response) : Results.Problem();
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("UpdateMedicalHistory")
            .WithTags("PatientProfiles")
            .Produces<UpdateMedicalHistoryResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Update Medical History")
            .WithSummary("Update Medical History");
    }
}