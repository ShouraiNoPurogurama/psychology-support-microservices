using Microsoft.AspNetCore.Mvc;

namespace Profile.API.Domains.Public.PatientProfiles.Features.GetTotalPatientProfile
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
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin", "Manager"))
            .WithName("GetTotalPatientProfile")
            .WithTags("PatientProfiles")
            .Produces<int>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get total number of patient profiles in the specified date range and gender.")
            .WithDescription("Retrieves the total count of patient profiles filtered by date range and gender. Requires 'User', 'Admin', or 'Manager' role.");
        }
    }
}
