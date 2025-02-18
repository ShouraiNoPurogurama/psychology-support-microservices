using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Pagination;
using Profile.API.DoctorProfiles.Models;
using Profile.API.DoctorProfiles.Dtos;

namespace Profile.API.DoctorProfiles.Features.GetDoctorProfiles
{
    public class GetDoctorProfilesEndpoint : ICarterModule
    {
        public record GetDoctorProfilesRequest(int PageIndex, int PageSize);

        public record GetDoctorProfilesResponse(PaginatedResult<DoctorProfileCreate> DoctorProfiles);
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("doctor-profiles", async ([FromQuery] int pageIndex, [FromQuery] int pageSize, ISender sender) =>
            {
                var query = new GetDoctorProfilesQuery(new PaginationRequest(pageIndex, pageSize));
                var result = await sender.Send(query);
                var response = result.Adapt<GetDoctorProfilesResponse>();
                return Results.Ok(response);
            })
            .WithName("GetDoctorProfiles")
            .Produces<GetDoctorProfilesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Doctor Profiles")
            .WithSummary("Get Doctor Profiles");
        }
    }
}
