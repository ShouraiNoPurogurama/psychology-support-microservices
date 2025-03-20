using BuildingBlocks.Enums;
using Carter;
using Microsoft.AspNetCore.Mvc;

namespace Profile.API.PatientProfiles.Features.GetTotalPatientProfile
{
    public class GetTotalPatientProfileEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/patients/total", async (
                [FromQuery] DateOnly startDate,
                [FromQuery] DateOnly endDate,
                [FromQuery] UserGender? gender,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new GetTotalPatientProfileQuery(startDate, endDate, gender);
                var totalPatients = await sender.Send(query, cancellationToken);
                return Results.Ok(new { TotalPatients = totalPatients });
            })
            .WithName("GetTotalPatientProfile")
            .Produces<int>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Total Patient Profiles")
            .WithDescription("GetTotalPatientProfile");
        }
    }
}
