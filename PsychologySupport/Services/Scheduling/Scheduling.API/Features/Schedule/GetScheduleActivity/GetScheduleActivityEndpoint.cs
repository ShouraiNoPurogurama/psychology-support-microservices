using Carter;
using Mapster;
using MediatR;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.Schedule.GetScheduleActivity
{
    public record GetScheduleActivityResponse(List<ScheduleActivityDto> ScheduleActivities);

    public class GetScheduleActivityEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/schedule-activities/{sessionId:guid}", async (Guid sessionId, ISender sender) =>
            {
                var query = new GetScheduleActivityQuery(sessionId);

                var result = await sender.Send(query);

                return Results.Ok(result.Adapt<GetScheduleActivityResponse>());
            })
                .WithName("GetScheduleActivity")
                .WithTags("Schedules")
                .Produces<GetScheduleActivityResponse>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Get Schedule Activities")
                .WithSummary("Get Schedule Activities");
        }
    }
}
