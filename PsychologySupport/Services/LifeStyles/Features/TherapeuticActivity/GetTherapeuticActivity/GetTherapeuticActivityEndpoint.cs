using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.TherapeuticActivity.GetTherapeuticActivity;

public record GetTherapeuticActivityRequest(Guid Id);
public record GetTherapeuticActivityResponse(TherapeuticActivityDto TherapeuticActivity);

public class GetTherapeuticActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/therapeutic-activities/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetTherapeuticActivityQuery(id);
            var result = await sender.Send(query);
            var response = result.Adapt<GetTherapeuticActivityResponse>();

            return Results.Ok(response);
        })
            .WithName("GetTherapeuticActivity")
            .Produces<GetTherapeuticActivityResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Therapeutic Activity")
            .WithSummary("Get Therapeutic Activity");
    }
}
