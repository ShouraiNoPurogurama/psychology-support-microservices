using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Profile.API.PatientProfiles.Models;
using System;
using System.Threading.Tasks;

public class GetMedicalHistoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients/{patientId}/medical-history", async (
            [FromRoute] Guid patientId,
            ISender sender) =>
        {
            var query = new GetMedicalHistoryQuery(patientId);
            var result = await sender.Send(query);

            if (result is null)
            {
                return Results.NotFound("Medical history not found.");
            }

            return Results.Ok(result);
        })
        .WithName("GetMedicalHistoryByPatientId")
        .Produces<MedicalHistory>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
