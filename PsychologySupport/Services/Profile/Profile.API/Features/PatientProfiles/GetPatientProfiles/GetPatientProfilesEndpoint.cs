using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Models;

namespace Profile.API.Features.PatientProfiles.GetPatientProfiles;

public record GetPatientProfilesRequest();

public record GetPatientProfilesResponse(IEnumerable<PatientProfile> PatientProfiles);

public class GetPatientProfilesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("patient-profiles", async ( ISender sender) =>
            {
                var query = new GetPatientProfilesQuery();

                var result = await sender.Send(query);

                var response = result.Adapt<GetPatientProfilesResponse>();

                return Results.Ok(response);
            })
            .WithName("GetPatientProfiles")
            .Produces<GetPatientProfilesResponse>()
            .WithDescription("Get Patient Profiles")
            .WithSummary("Get Patient Profiles");
    }
}