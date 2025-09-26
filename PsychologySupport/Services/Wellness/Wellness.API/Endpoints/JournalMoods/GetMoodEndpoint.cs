using Carter;
using MediatR;
using Wellness.Application.Features.JournalMoods.Dtos;
using Wellness.Application.Features.JournalMoods.Queries;

namespace Wellness.API.Endpoints.JournalMoods;

public record GetMoodsResponse(IEnumerable<MoodDto> Moods);

public class GetMoodEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/moods", async (ISender sender) =>
        {
            var result = await sender.Send(new GetMoodsQuery());
            return Results.Ok(new GetMoodsResponse(result.Moods));
        })
        .WithName("GetMoods")
        .WithTags("Moods")
        .Produces<GetMoodsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get all moods")
        .WithDescription("Returns the full list of available moods.");
    }
}
