using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.EntertainmentActivity.GetEntertainmentActivity;

public record GetEntertainmentActivityRequest(Guid Id);

public record GetEntertainmentActivityResponse(EntertainmentActivityDto EntertainmentActivity);

public class GetEntertainmentActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/entertainment-activities/{id:guid}", async (Guid id, ISender sender) =>
            {
                var query = new GetEntertainmentActivityQuery(id);
                var result = await sender.Send(query);
                var response = result.Adapt<GetEntertainmentActivityResponse>();
                return Results.Ok(response);
            })
            .WithName("GetEntertainmentActivity")
            .Produces<GetEntertainmentActivityResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Entertainment Activity")
            .WithSummary("Get Entertainment Activity");
    }
}