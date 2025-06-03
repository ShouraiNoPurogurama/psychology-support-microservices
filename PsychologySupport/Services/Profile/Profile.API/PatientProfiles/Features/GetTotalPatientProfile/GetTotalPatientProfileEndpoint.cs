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
                [FromQuery] string? gender,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                UserGender? userGender = null;
                if (!string.IsNullOrEmpty(gender) && System.Enum.TryParse<UserGender>(gender, true, out var parsedGender))
                {
                    userGender = parsedGender;
                }
                
                var query = new GetTotalPatientProfileQuery(startDate, endDate, userGender);
                var totalPatients = await sender.Send(query, cancellationToken);
                return Results.Ok(new { TotalPatients = totalPatients });
            })
            .WithName("GetTotalPatientProfile")
            .WithTags("PatientProfiles")
            .Produces<int>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Total Patient Profiles")
            .WithDescription("GetTotalPatientProfile");
        }
    }
}
