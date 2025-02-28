using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.PhysicalActivity.GetPhysicalActivity;

public record GetPhysicalActivityRequest(Guid Id);

public record GetPhysicalActivityResponse(PhysicalActivityDto PhysicalActivity);

public class GetPhysicalActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/physical-activities/{id:guid}", async (Guid id, ISender sender) =>
            {
                var query = new GetPhysicalActivityQuery(id);
                var result = await sender.Send(query);
                var response = result.Adapt<GetPhysicalActivityResponse>();

                return Results.Ok(response);
            })
            .WithName("GetPhysicalActivity")
            .Produces<GetPhysicalActivityResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Physical Activity")
            .WithSummary("Get Physical Activity");
    }
}