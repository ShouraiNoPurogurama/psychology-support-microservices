using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pricing.API.Modules;

namespace Pricing.API.Features.ExperiencePriceRanges.GetExperiencePriceRanges;

public record GetExperiencePriceRangesRequest(int PageNumber, int PageSize);

public record GetExperiencePriceRangesResponse(IEnumerable<ExperiencePriceRange> ExperiencePriceRanges, int TotalCount);

public class GetExperiencePriceRangesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/experience-price-ranges", async ([FromQuery] int pageNumber, [FromQuery] int pageSize, ISender sender) =>
        {
            var query = new GetExperiencePriceRangesQuery(pageNumber, pageSize);

            var result = await sender.Send(query);

            var response = result.Adapt<GetExperiencePriceRangesResponse>();

            return Results.Ok(response);
        })
        .WithName("GetExperiencePriceRanges")
        .Produces<GetExperiencePriceRangesResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Get Experience Price Ranges")
        .WithSummary("Get Experience Price Ranges");
    }
}
