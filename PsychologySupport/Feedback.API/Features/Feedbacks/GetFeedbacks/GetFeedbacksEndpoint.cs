using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Pagination;

namespace Blog.API.Features.Feedbacks.GetFeedbacks;

public record GetFeedbacksRequest(int PageIndex, int PageSize);

public record GetFeedbacksResponse(PaginatedResult<Feedback.API.Models.Feedback> Feedbacks);

public class GetFeedbacksEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("feedbacks", async ([FromQuery] int pageIndex, [FromQuery] int pageSize, ISender sender) =>
        {
            var query = new GetFeedbacksQuery(new PaginationRequest(pageIndex, pageSize));
            var result = await sender.Send(query);

            var response = result.Adapt<GetFeedbacksResponse>();
            return Results.Ok(response);
        })
        .WithName("GetFeedbacks")
        .Produces<GetFeedbacksResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Get Feedbacks")
        .WithSummary("Get Feedbacks");
    }
}
