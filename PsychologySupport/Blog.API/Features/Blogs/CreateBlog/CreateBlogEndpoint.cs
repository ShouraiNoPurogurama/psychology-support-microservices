using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Features.Blogs.CreateBlog;

public record CreateBlogRequest(Models.Blog Blog);

public record CreateBlogResponse(Guid Id);

public class CreateBlogEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("blog", async ([FromBody] CreateBlogRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateBlogCommand>();

            var result = await sender.Send(command);

            var response = result.Adapt<CreateBlogResponse>();

            return Results.Created($"/blogs/{response.Id}", response);
        }
        )
        .WithName("CreateBlog")
        .Produces<CreateBlogResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create Blog")
        .WithSummary("Create Blog");
    }
}
