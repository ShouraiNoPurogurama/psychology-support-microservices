using BuildingBlocks.CQRS;
using Carter;
using MediatR;
using Scheduling.API.Dtos;
using Scheduling.API.Features.Schedule.GetTotalSession;

namespace Scheduling.API.Features.Schedule.GetTotalActivities
{
    public record GetTotalActivitiesResponse(TotalActivityDto Activities);

    public class GetTotalActivitiesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/schedule-activities/get-total-activities", async ([AsParameters] GetTotalActivitiesQuery request, ISender sender) =>
            {
                var result = await sender.Send(request);
                var response = new GetTotalActivitiesResponse(result);
                return Results.Ok(response);
            })
            .WithName("GetTotalActivities")
            .WithTags("Schedules")
            .Produces<GetTotalActivitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get total activity times within a date range")
            .WithSummary("Get Total Activities");
        }
    }


}
