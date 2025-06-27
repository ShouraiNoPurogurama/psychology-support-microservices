using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Common;

namespace Scheduling.API.Features.Schedule.CreateScheduleFeedback
{
    public record CreateScheduleFeedbackResponse(bool IsSucceeded);

    public class CreateScheduleFeedbackEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("schedule-feedback",
                async ([FromBody] CreateScheduleFeedbackCommand request, ISender sender, HttpContext httpContext) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientId, httpContext.User))
                        return Results.Problem(
                              statusCode: StatusCodes.Status403Forbidden,
                              title: "Forbidden",
                              detail: "You do not have permission to access this resource."
                          );

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
                .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
                .WithName("CreateScheduleFeedback")
                .WithTags("Schedules")
                .Produces<CreateScheduleFeedbackResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Create feedback for a schedule")
                .WithSummary("Create feedback for a schedule");
        }
    }
}

