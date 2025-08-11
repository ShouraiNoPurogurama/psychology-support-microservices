using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Common;

namespace Scheduling.API.Features02.Schedule.CreateScheduleFeedback
{
    public record CreateScheduleFeedbackV2Request(
        Guid ScheduleId,
        Guid PatientId,
        string Content,
        int Rating,
        DateTime FeedbackDate
    );

    public record CreateScheduleFeedbackV2Response(bool IsSucceeded);

    public class CreateScheduleFeedbackV2Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/v2/me/scheduleFeedback",
                async ([FromBody] CreateScheduleFeedbackV2Request request, ISender sender, HttpContext httpContext) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientId, httpContext.User))
                        return Results.Problem(
                              statusCode: StatusCodes.Status403Forbidden,
                              title: "Forbidden",
                              detail: "You do not have permission to access this resource."
                          );

                    // Map từ request DTO sang Command
                    var command = new CreateScheduleFeedbackCommand(
                        request.ScheduleId,
                        request.PatientId,
                        request.Content,
                        request.Rating,
                        request.FeedbackDate
                    );

                    var result = await sender.Send(command);

                    var response = new CreateScheduleFeedbackV2Response(result.IsSucceeded);

                    return Results.Created($"/v2/{request.ScheduleId}/scheduleFeedback", response);

                })
                .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
                .WithName("CreateScheduleFeedback v2")
                .WithTags("Schedules Version 2")
                .Produces<CreateScheduleFeedbackV2Response>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Create feedback for a schedule")
                .WithSummary("Create feedback for a schedule");
        }
    }
}
