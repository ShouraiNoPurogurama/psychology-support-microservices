using Carter;
using Mapster;
using MediatR;
using Blog.API.Models;

namespace Blog.API.Features.Blogs.GetBlog;

public record GetBlogResponse(Models.Blog Blog);

public class GetBlogEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/blog/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetBlogQuery(id);

            var result = await sender.Send(query);

            var response = result.Adapt<GetBlogResponse>();

            return Results.Ok(response);
        })
            .WithName("GetBlog")
            .Produces<GetBlogResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Blog")
            .WithSummary("Get Blog");
    }
}
