using Carter;
using Mapster;
using MediatR;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features02.Schedule.GetScheduleActivity
{
    public record GetScheduleActivityV2Response(List<ScheduleActivityDto> ScheduleActivities);

    public class GetScheduleActivityV2Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/v2/me/sessions/{sessionId:guid}/activities", async (Guid sessionId, ISender sender) =>
            {
                var query = new GetScheduleActivityQuery(sessionId);

                var result = await sender.Send(query);

                return Results.Ok(result.Adapt<GetScheduleActivityV2Response>());
            })
                .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
                .WithName("GetScheduleActivity v2")
                .WithTags("Schedules Version 2")
                .Produces<GetScheduleActivityV2Response>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Get Schedule Activities")
                .WithSummary("Get Schedule Activities");
        }
    }
}
