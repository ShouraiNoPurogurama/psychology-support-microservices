using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.Schedule.EditScheduleActivity
{
    public record EditScheduleActivityRequest(EditScheduleActivityDto ActivityDto);

    public record EditScheduleActivityResponse(Guid Id, Guid SessionId);

    public class EditScheduleActivityEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/schedule-activities/{id}/{sessionId}",
                    async ([FromRoute] Guid id, [FromRoute] Guid sessionId, [FromBody] EditScheduleActivityRequest request, ISender sender) =>
                    {
                        var command = new EditScheduleActivityCommand(id, sessionId, request.ActivityDto);
                        var result = await sender.Send(command);
                        var response = result.Adapt<EditScheduleActivityResponse>();
                        return Results.Ok(response);
                    })
                .WithName("Edit Schedule Activity")
                .WithTags("Schedules")
                .Produces<EditScheduleActivityResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("Edit Schedule Activity")
                .WithSummary("Edit Schedule Activity");
        }
    }
}
