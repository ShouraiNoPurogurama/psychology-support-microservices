using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Models;


namespace Profile.API.Features.PatientProfiles.GetPatientProfiles;

public record GetPatientProfilesRequest(int PageNumber, int PageSize);

public record GetPatientProfilesResponse(IEnumerable<PatientProfile> PatientProfiles, int TotalCount);

public class GetPatientProfilesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("patient-profiles", async ([FromQuery] int pageNumber, [FromQuery] int pageSize, ISender sender) =>
        {
            /*if (pageNumber <= 0 || pageSize <= 0)
            {
                return Results.BadRequest("");
            }*/

            var query = new GetPatientProfilesQuery(pageNumber, pageSize);
            var result = await sender.Send(query);

            var response = result.Adapt<GetPatientProfilesResponse>();
            return Results.Ok(response);
        })
            .WithName("GetPatientProfiles")
            .Produces<GetPatientProfilesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Patient Profiles")
            .WithSummary("Get Patient Profiles");
    }
}
