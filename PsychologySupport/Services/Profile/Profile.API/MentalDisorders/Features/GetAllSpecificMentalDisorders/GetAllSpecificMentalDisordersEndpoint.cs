using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using Profile.API.MentalDisorders.Dtos;

namespace Profile.API.MentalDisorders.Features.GetAllSpecificMentalDisorders
{
    public record GetAllSpecificMentalDisordersResponse(PaginatedResult<SpecificMentalDisorderDto> PaginatedResult);

    public class GetAllSpecificMentalDisordersEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/specific-mental-disorders", async (
                    [AsParameters] PaginationRequest request,
                    ISender sender) =>
            {
                var query = new GetAllSpecificMentalDisordersQuery(request);
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllSpecificMentalDisordersResponse>();

                return Results.Ok(response);
            })
            .WithName("GetAllSpecificMentalDisorders")
            .Produces<GetAllSpecificMentalDisordersResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Get All Specific Mental Disorders")
            .WithSummary("Get All Specific Mental Disorders");
        }
    }

}
