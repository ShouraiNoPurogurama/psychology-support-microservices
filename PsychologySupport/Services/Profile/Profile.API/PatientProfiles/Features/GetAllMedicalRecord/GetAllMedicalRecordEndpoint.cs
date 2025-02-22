using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public record GetAllMedicalRecordsResponse(IEnumerable<MedicalRecordDto> MedicalRecords, int TotalRecords);

public class GetAllMedicalRecordsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients/{patientId}/medical-records", async (
        [FromRoute] Guid patientId,  
        [FromQuery] int? pageNumber,  
        [FromQuery] int? pageSize,
        ISender sender) =>
        {
            var query = new GetAllMedicalRecordsQuery(patientId, pageNumber ?? 1, pageSize ?? 10);
            var result = await sender.Send(query);

            var response = new GetAllMedicalRecordsResponse(result.MedicalRecords, result.TotalRecords);

            return Results.Ok(response);
        })
        .WithName("GetAllMedicalRecordsByPatientId")
        .Produces<GetAllMedicalRecordsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
