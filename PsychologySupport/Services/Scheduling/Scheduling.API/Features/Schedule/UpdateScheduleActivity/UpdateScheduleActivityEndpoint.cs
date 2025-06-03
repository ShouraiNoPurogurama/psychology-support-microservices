using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Enums;

namespace Scheduling.API.Features.Schedule.UpdateScheduleActivity
{
    public record UpdateScheduleActivityRequest(ScheduleActivityStatus Status);

    public record UpdateScheduleActivityResponse(Guid Id, Guid SessionId);

    public class UpdateScheduleActivityEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/schedule-activities/{id}/{sessionId}/status",
                    async ([FromRoute] Guid id, [FromRoute] Guid sessionId, [FromBody] UpdateScheduleActivityRequest request, ISender sender) =>
                    {
                        var command = new UpdateScheduleActivityCommand(id, sessionId, request.Status);
                        var result = await sender.Send(command);
                        var response = result.Adapt<UpdateScheduleActivityResponse>();
                        return Results.Ok(response);
                    })
                .WithName("Update Schedule Activity Status")
                .WithTags("Schedules")
                .Produces<UpdateScheduleActivityResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("Update Schedule Activity Status")
                .WithSummary("Update Schedule Activity Status");
        }
    }
}
