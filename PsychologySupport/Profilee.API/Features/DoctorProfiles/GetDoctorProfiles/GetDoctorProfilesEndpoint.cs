using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Profilee.API.Models;

namespace Profilee.API.Features.DoctorProfiles.GetDoctorProfiles
{
    public record GetDoctorProfilesRequest();
    public record GetDoctorProfilesResponse(IEnumerable<DoctorProfile> DoctorProfiles);

    public class GetDoctorProfilesEndpoint : ICarterModule
    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("doctor-profiles",async( ISender sender) =>
            {
                var query = new GetDoctorProfilesQuery();

                var result =  await sender.Send(query);

                var respone = result.Adapt<GetDoctorProfilesResponse>();

                return Results.Ok(respone);

    
            })
            .WithName("GetDoctorProfiles")
            .Produces<GetDoctorProfilesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Doctor Profiles")
            .WithSummary("Get Doctor Profiles");
        }
    }
}
