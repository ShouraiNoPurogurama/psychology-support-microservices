using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Features.Feedbacks.DeleteFeedback;

public record DeleteFeedbackRequest(Guid Id);

public record DeleteFeedbackResponse(bool IsSuccess);

public class DeleteFeedbackEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("feedback", async ([FromBody] DeleteFeedbackRequest request, ISender sender) =>
        {
            var result = await sender.Send(new DeleteFeedbackCommand(request.Id));

            var response = result.Adapt<DeleteFeedbackResponse>();

            return Results.Ok(response);
        })
        .WithName("DeleteFeedback")
        .Produces<DeleteFeedbackResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Delete Feedback")
        .WithSummary("Delete Feedback");
    }
}
