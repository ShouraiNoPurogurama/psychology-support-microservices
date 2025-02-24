using BuildingBlocks.Pagination;
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
        [AsParameters] PaginationRequest request,
        ISender sender) =>
        {
            var query = new GetAllMedicalRecordsQuery(patientId, request);
            var result = await sender.Send(query);

            var response = new GetAllMedicalRecordsResponse(result.MedicalRecords, result.TotalRecords);

            return Results.Ok(response);
        })
        .WithName("GetAllMedicalRecordsByPatientId")
        .Produces<GetAllMedicalRecordsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("GetAll MedicalRecords By PatientId")
        .WithSummary("GetAll MedicalRecords By PatientId");


    }
}
