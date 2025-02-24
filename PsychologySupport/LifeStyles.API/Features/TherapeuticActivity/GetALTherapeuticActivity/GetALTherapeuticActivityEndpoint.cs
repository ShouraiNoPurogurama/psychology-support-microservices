using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using LifeStyles.API.Dtos;

namespace LifeStyles.API.Features.TherapeuticActivity.GetAllTherapeuticActivity;

public record GetAllTherapeuticActivitiesResponse(IEnumerable<TherapeuticActivityDto> TherapeuticActivities);

public class GetAllTherapeuticActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/therapeutic-activities", async ([AsParameters] PaginationRequest request, ISender sender) =>
        {
            var query = new GetAllTherapeuticActivitiesQuery(request);
            var result = await sender.Send(query);
            var response = result.Adapt<GetAllTherapeuticActivitiesResponse>();

            return Results.Ok(response);
        })
        .WithName("GetAllTherapeuticActivities")
        .Produces<GetAllTherapeuticActivitiesResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Get All Therapeutic Activities")
        .WithSummary("Get All Therapeutic Activities");
    }
}
