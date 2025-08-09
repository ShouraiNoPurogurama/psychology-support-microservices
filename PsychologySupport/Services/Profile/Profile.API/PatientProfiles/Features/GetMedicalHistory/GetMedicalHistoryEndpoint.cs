using Carter;
using Mapster;
using Microsoft.AspNetCore.Http;
using Profile.API.Common.Helpers;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.GetMedicalHistory;

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
            .WithTags("PatientProfiles")
            .Produces<GetMedicalHistoryResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Get MedicalHistory By PatientId")
            .WithSummary("Get MedicalHistory By PatientId");
    }
}