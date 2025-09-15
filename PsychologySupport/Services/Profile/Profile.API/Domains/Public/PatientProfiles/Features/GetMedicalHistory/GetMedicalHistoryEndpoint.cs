using Profile.API.Common.Helpers;
using Profile.API.Domains.Public.PatientProfiles.Dtos;

namespace Profile.API.Domains.Public.PatientProfiles.Features.GetMedicalHistory;

public record GetMedicalHistoryRequest(Guid PatientId);

public record GetMedicalHistoryResponse(MedicalHistoryDto History);

public class GetMedicalHistoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients/{patientId:guid}/medical-history", async (Guid patientId, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(patientId, httpContext.User))
                    throw new ForbiddenException();

                var query = new GetMedicalHistoryQuery(patientId);
                var result = await sender.Send(query);
                var response = result.Adapt<GetMedicalHistoryResponse>();
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetMedicalHistoryByPatientId")
            .WithTags("MedicalHistory")
            .Produces<GetMedicalHistoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves the medical history for the specified patient ID. Requires 'User' or 'Admin' role. Returns medical history details.")
            .WithSummary("Get medical history by patient ID");
    }
}