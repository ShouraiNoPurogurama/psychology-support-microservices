using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.TherapeuticActivity.GetALTherapeuticActivity;

public record GetAllTherapeuticActivitiesResponse(PaginatedResult<TherapeuticActivityDto> TherapeuticActivities);

public class GetAllTherapeuticActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/therapeutic-activities", async ([AsParameters] GetAllTherapeuticActivitiesQuery request, ISender sender) =>
        {
            var result = await sender.Send(request);
            var response = result.Adapt<GetAllTherapeuticActivitiesResponse>();

            return Results.Ok(response);
        })
            .WithName("GetAllTherapeuticActivities")
            .Produces<GetAllTherapeuticActivitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get all therapeutic activities with pagination")
            .WithSummary("Get Paginated Therapeutic Activities");
    }
}