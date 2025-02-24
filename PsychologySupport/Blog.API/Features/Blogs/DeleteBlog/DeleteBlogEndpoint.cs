using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Features.Blogs.DeleteBlog;

public record DeleteBlogRequest(Guid Id);

public record DeleteBlogResponse(bool IsSuccess);

public class DeleteBlogEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("blog", async ([FromBody] DeleteBlogRequest request, ISender sender) =>
        {
            var result = await sender.Send(new DeleteBlogCommand(request.Id));

            var response = result.Adapt<DeleteBlogResponse>();

            return Results.Ok(response);
        })
        .WithName("DeleteBlog")
        .Produces<DeleteBlogResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Delete Blog")
        .WithSummary("Delete Blog");
    }
}
