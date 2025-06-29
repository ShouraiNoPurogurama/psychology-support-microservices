using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Profile.API.Common.Helpers;
using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Enum;

namespace Profile.API.PatientProfiles.Features.GetAllMedicalRecord;

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
            // Authorization check
            if (request.PatientId is Guid patientId)
            {
                if (!AuthorizationHelpers.CanViewPatientProfile(patientId, httpContext.User))
                    return Results.Problem(
                              statusCode: StatusCodes.Status403Forbidden,
                              title: "Forbidden",
                              detail: "You do not have permission to access this resource."
                          );
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
