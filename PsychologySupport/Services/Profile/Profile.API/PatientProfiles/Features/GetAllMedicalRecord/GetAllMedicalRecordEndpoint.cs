using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.GetAllMedicalRecord;

public record GetAllMedicalRecordsResponse(PaginatedResult<MedicalRecordDto> MedicalRecords);

public class GetAllMedicalRecordsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients/{patientId}/medical-records", async (
                [FromRoute] Guid patientId,
                [AsParameters] PaginationRequest request,
                ISender sender) =>
        {
            var query = new GetAllMedicalRecordsQuery(patientId, request);
            var result = await sender.Send(query);
            var response = result.Adapt<GetAllMedicalRecordsResponse>();

            return Results.Ok(response);
        })
            .WithName("GetAllMedicalRecords")
            .Produces<GetAllMedicalRecordsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Get All MedicalRecords")
            .WithSummary("Get All MedicalRecords");
    }
}
