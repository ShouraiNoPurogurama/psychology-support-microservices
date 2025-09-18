using Profile.API.Common.Helpers;
using Profile.API.Domains.Public.PatientProfiles.Dtos;

namespace Profile.API.Domains.Public.PatientProfiles.Features.GetMedicalRecord
{
    public record GetMedicalRecordRequest(Guid MedicalRecordId);

    public record GetMedicalRecordResponse(MedicalRecordDto Record);

    public class GetMedicalRecordEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/medical-records/{medicalRecordId:guid}", async (Guid medicalRecordId,HttpContext httpContext, ISender sender) =>
            {
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    throw new ForbiddenException();
                
                var query = new GetMedicalRecordQuery(medicalRecordId);
                var result = await sender.Send(query);
                var response = result.Adapt<GetMedicalRecordResponse>();
                return Results.Ok(response);
            })
                .RequireAuthorization(policy => policy.RequireRole("Doctor", "Admin"))
                .WithName("GetMedicalRecordById")
                .WithTags("MedicalRecords")
                .Produces<GetMedicalRecordResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("Retrieves the medical record for the specified ID. Requires 'Doctor' or 'Admin' role. Returns medical record details.")
                .WithSummary("Get medical record by ID");
        }
    }
}
