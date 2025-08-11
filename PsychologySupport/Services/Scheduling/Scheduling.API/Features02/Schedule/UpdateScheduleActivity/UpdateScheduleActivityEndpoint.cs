using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Enums;

namespace Scheduling.API.Features02.Schedule.UpdateScheduleActivity
{
    public record UpdateScheduleActivityV2Request(ScheduleActivityStatus Status);

    public record UpdateScheduleActivityV2Response(Guid Id, Guid SessionId);

    public class UpdateScheduleActivityV2Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/v2/me/scheduleActivities/{id}/session/{sessionId}/status",
                    async ([FromRoute] Guid id, [FromRoute] Guid sessionId, [FromBody] UpdateScheduleActivityV2Request request, ISender sender) =>
                    {
                        var command = new UpdateScheduleActivityCommand(id, sessionId, request.Status);
                        var result = await sender.Send(command);
                        var response = result.Adapt<UpdateScheduleActivityV2Response>();
                        return Results.Ok(response);
                    })
                .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
                .WithName("Update Schedule Activity Status v2")
                .WithTags("Schedules Version 2")
                .Produces<UpdateScheduleActivityV2Response>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("Update Schedule Activity Status")
                .WithSummary("Update Schedule Activity Status");
        }
    }
}
