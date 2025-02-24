using Carter;
using Mapster;
using MediatR;
using Pricing.API.Models;

namespace Pricing.API.Features.ExperiencePriceRanges.GetExperiencePriceRange;

public record GetExperiencePriceRangeResponse(ExperiencePriceRange ExperiencePriceRange);

public class GetExperiencePriceRangeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/experience-price-range/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetExperiencePriceRangeQuery(id);

            var result = await sender.Send(query);

            var response = result.Adapt<GetExperiencePriceRangeResponse>();

            return Results.Ok(response);
        })
            .WithName("GetExperiencePriceRange")
            .Produces<GetExperiencePriceRangeResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Experience Price Range")
            .WithSummary("Get Experience Price Range");
    }
}
