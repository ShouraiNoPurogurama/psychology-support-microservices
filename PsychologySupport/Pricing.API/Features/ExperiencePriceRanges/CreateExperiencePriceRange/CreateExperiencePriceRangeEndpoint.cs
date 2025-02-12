using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pricing.API.Models;  

namespace Pricing.API.Features.ExperiencePriceRanges.CreateExperiencePriceRange;

public record CreateExperiencePriceRangeRequest(ExperiencePriceRange ExperiencePriceRange);

public record CreateExperiencePriceRangeResponse(Guid Id);

public class CreateExperiencePriceRangeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/experience-price-ranges", async ([FromBody] CreateExperiencePriceRangeRequest request, ISender sender) =>
        {
            var command = new CreateExperiencePriceRangeCommand(request.ExperiencePriceRange);

            var result = await sender.Send(command);

            var response = result.Adapt<CreateExperiencePriceRangeResponse>();

            return Results.Created($"/experience-price-ranges/{response.Id}", response);
        })
        .WithName("CreateExperiencePriceRange")
        .Produces<CreateExperiencePriceRangeResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create Experience Price Range")
        .WithSummary("Create Experience Price Range");
    }
}
