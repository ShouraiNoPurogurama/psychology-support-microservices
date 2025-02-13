using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Feedback.API.Models;

namespace Blog.API.Features.Feedbacks.CreateFeedback;

public record CreateFeedbackRequest(Feedback.API.Models.Feedback Feedback);

public record CreateFeedbackResponse(Guid Id);

public class CreateFeedbackEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("feedback", async ([FromBody] CreateFeedbackRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateFeedbackCommand>();

            var result = await sender.Send(command);

            var response = result.Adapt<CreateFeedbackResponse>();

            return Results.Created($"/feedbacks/{response.Id}", response);
        }
        )
        .WithName("CreateFeedback")
        .Produces<CreateFeedbackResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create Feedback")
        .WithSummary("Create Feedback");
    }
}
