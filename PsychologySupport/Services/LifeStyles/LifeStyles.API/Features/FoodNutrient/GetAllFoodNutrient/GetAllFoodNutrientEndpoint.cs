using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.FoodNutrient.GetAllFoodNutrient
{
    public record GetAllFoodNutrientResponse(PaginatedResult<FoodNutrientDto> FoodNutrients);

    public class GetAllFoodNutrientEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/food-nutrients", async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var query = new GetAllFoodNutrientQuery(request);
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllFoodNutrientResponse>();

                return Results.Ok(response);
            })
            .WithName("GetAllFoodNutrients")
            .WithTags("FoodNutrients")
            .Produces<GetAllFoodNutrientResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get all food nutrients with pagination")
            .WithSummary("Get Paginated Food Nutrients");
        }
    }
}
