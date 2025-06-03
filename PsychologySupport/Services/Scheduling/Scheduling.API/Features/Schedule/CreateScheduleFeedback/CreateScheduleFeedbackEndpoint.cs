using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Scheduling.API.Features.Schedule.CreateScheduleFeedback
{
    public record CreateScheduleFeedbackResponse(bool IsSucceeded);

    public class CreateScheduleFeedbackEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("schedule-feedback",
                async ([FromBody] CreateScheduleFeedbackCommand request, ISender sender) =>
                {
                    var command = new CreateScheduleFeedbackCommand(
                        request.ScheduleId,
                        request.PatientId,
                        request.Content,
                        request.Rating,
                        request.FeedbackDate
                    );

                    var result = await sender.Send(command);

                    var response = new CreateScheduleFeedbackResponse(result.IsSucceeded);

                    return Results.Created($"/schedule-feedback/{request.ScheduleId}", response);

                })
                .WithName("CreateScheduleFeedback")
                .WithTags("Schedules")
                .Produces<CreateScheduleFeedbackResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Create feedback for a schedule")
                .WithSummary("Create feedback for a schedule");
        }
    }
}

