using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Pricing.API.Features.ExperiencePriceRanges.DeleteExperiencePriceRange;

public record DeleteExperiencePriceRangeRequest(Guid Id);

public record DeleteExperiencePriceRangeResponse(bool IsSuccess);

public class DeleteExperiencePriceRangeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/experience-price-ranges", async (Guid id, ISender sender) =>
        {
            var command = new DeleteExperiencePriceRangeCommand(id);

            var result = await sender.Send(command);

            var response = result.Adapt<DeleteExperiencePriceRangeResponse>();

            return response.IsSuccess ? Results.Ok(response) : Results.NotFound(response);
        })
        .WithName("DeleteExperiencePriceRange")
        .Produces<DeleteExperiencePriceRangeResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("Delete Experience Price Range")
        .WithSummary("Delete Experience Price Range");
    }
}
