using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features02.Schedule.EditScheduleActivity
{
    public record EditScheduleActivityV2Request(EditScheduleActivityDto ActivityDto);

    public record EditScheduleActivityV2Response(Guid Id, Guid SessionId);

    public class EditScheduleActivityV2Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/v2/schedule/activities/{id}/session/{sessionId}",
                    async ([FromRoute] Guid id, [FromRoute] Guid sessionId, [FromBody] EditScheduleActivityV2Request request, ISender sender) =>
                    {
                        var command = new EditScheduleActivityCommand(id, sessionId, request.ActivityDto);
                        var result = await sender.Send(command);
                        var response = result.Adapt<EditScheduleActivityV2Response>();
                        return Results.Ok(response);
                    })
                .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
                .WithName("Edit Schedule Activity v2")
                .WithTags("Schedules Version 2")
                .Produces<EditScheduleActivityV2Response>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("Edit Schedule Activity")
                .WithSummary("Edit Schedule Activity");
        }
    }
}
