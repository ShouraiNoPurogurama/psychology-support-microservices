using Carter;
using MediatR;
using Wellness.Application.Features.JournalMoods.Dtos;
using Wellness.Application.Features.JournalMoods.Queries;

namespace Wellness.API.Endpoints.JournalMoods;

public record GetMoodsRequest(
    string? TargetLang
);

public record GetMoodsResponse(IReadOnlyList<MoodDto> Moods);

public class GetMoodsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/moods", async (
            [AsParameters] GetMoodsRequest request,
            ISender sender
        ) =>
        {

            var query = new GetMoodsQuery(
                TargetLang: request.TargetLang
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetMoodsResponse(result.Moods.ToList()));
        })
        //.RequireAuthorization() // bật nếu cần bảo vệ route
        .WithName("GetMoods")
        .WithTags("Moods")
        .Produces<GetMoodsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get all moods")
        .WithDescription("Returns the list of available moods, optionally translated by TargetLang.");
    }
}
