using Carter;
using Mapster;
using MediatR;

namespace Blog.API.Features.Feedbacks.GetFeedback;

public record GetFeedbackResponse(Feedback.API.Models.Feedback Feedback);

public class GetFeedbackEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/feedback/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetFeedbackQuery(id);

            var result = await sender.Send(query);

            var response = result.Adapt<GetFeedbackResponse>();

            return Results.Ok(response);
        })
        .WithName("GetFeedback")
        .Produces<GetFeedbackResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Get Feedback")
        .WithSummary("Get Feedback");
    }
}
