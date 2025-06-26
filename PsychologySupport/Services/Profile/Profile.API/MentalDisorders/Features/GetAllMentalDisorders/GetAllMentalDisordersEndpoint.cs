using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using Profile.API.MentalDisorders.Dtos;

namespace Profile.API.MentalDisorders.Features.GetAllMentalDisorders
{
    public record GetAllMentalDisordersResponse(PaginatedResult<MentalDisorderDto> PaginatedResult);

    public class GetAllMentalDisordersEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/mental-disorders", async (
                    [AsParameters] PaginationRequest request,
                    ISender sender) =>
            {
                var query = new GetAllMentalDisordersQuery(request);
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllMentalDisordersResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin","Doctor"))
            .WithName("GetAllMentalDisorders")
            .WithTags("MentalDisorders")
            .Produces<GetAllMentalDisordersResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Get All Mental Disorders")
            .WithSummary("Get All Mental Disorders");
        }

    }
}
