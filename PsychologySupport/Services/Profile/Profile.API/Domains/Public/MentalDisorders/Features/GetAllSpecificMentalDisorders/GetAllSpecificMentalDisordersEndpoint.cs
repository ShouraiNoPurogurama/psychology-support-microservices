using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Domains.MentalDisorders.Dtos;

namespace Profile.API.Domains.MentalDisorders.Features.GetAllSpecificMentalDisorders
{
    public record GetAllSpecificMentalDisordersResponse(PaginatedResult<SpecificMentalDisorderDto> SpecificMentalDisorder);

    public class GetAllSpecificMentalDisordersEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/specific-mental-disorders", async (
                [AsParameters] PaginationRequest request,
                [FromQuery] string? search,
                ISender sender) =>
            {
                var query = new GetAllSpecificMentalDisordersQuery(request, search);
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllSpecificMentalDisordersResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin","Doctor"))
            .WithName("GetAllSpecificMentalDisorders")
            .WithTags("SpecificMentalDisorders")
            .Produces<GetAllSpecificMentalDisordersResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("GetAllSpecificMentalDisorders")
            .WithSummary("GetAllSpecificMentalDisorders");
        }
    }
}
