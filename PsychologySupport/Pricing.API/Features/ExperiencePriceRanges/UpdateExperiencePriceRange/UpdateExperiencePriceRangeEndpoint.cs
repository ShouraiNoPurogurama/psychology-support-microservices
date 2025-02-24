using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pricing.API.Dtos;

namespace Pricing.API.Features.ExperiencePriceRanges.UpdateExperiencePriceRange;

public record UpdateExperiencePriceRangeRequest(ExperiencePriceRangeDto ExperiencePriceRange);

public record UpdateExperiencePriceRangeResponse(bool IsSuccess);

public class UpdateExperiencePriceRangeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/experience-price-range", async ([FromBody] UpdateExperiencePriceRangeRequest request, ISender sender) =>
        {
            var command = request.Adapt<UpdateExperiencePriceRangeCommand>();

            var result = await sender.Send(command);

            var response = result.Adapt<UpdateExperiencePriceRangeResponse>();

            return Results.Ok(response);
        })
        .WithName("UpdateExperiencePriceRange")
        .Produces<UpdateExperiencePriceRangeResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Update Experience Price Range")
        .WithSummary("Update Experience Price Range");
    }
}
