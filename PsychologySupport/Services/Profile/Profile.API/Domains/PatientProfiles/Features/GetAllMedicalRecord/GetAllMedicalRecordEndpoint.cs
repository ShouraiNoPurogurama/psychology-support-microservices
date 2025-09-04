using BuildingBlocks.Pagination;
using Profile.API.Common.Helpers;
using Profile.API.Domains.PatientProfiles.Dtos;
using Profile.API.Domains.PatientProfiles.Enum;

namespace Profile.API.Domains.PatientProfiles.Features.GetAllMedicalRecord;

public class GetAllMedicalRecordsEndpoint : ICarterModule
{
    public record class GetAllMedicalRecordsRequest(
     int PageIndex = 1,
     int PageSize = 10,
     string? Search = null,
     string? SortBy = "CreatedAt",
     string? SortOrder = "asc",
     Guid? PatientId = null,
     Guid? DoctorId = null,
     MedicalRecordStatus? Status = null
    );

    public record GetAllMedicalRecordsResponse(PaginatedResult<MedicalRecordDto> MedicalRecords);
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/medical-records", async (
            [AsParameters] GetAllMedicalRecordsRequest request,
            ISender sender, HttpContext httpContext) =>
        {
            if (request.PatientId is { } patientId)
            {
                if (!AuthorizationHelpers.CanViewPatientProfile(patientId, httpContext.User))
                    throw new ForbiddenException();
            }

            var query = request.Adapt<GetAllMedicalRecordsQuery>();

            var result = await sender.Send(query);
            var response = new GetAllMedicalRecordsResponse(result.MedicalRecords);
            return Results.Ok(response);
        })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithName("GetAllMedicalRecords")
        .WithTags("PatientProfiles")
        .Produces<GetAllMedicalRecordsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get All Medical Records")
        .WithDescription("Returns paginated list of medical records based on optional filters.");
    }
}
