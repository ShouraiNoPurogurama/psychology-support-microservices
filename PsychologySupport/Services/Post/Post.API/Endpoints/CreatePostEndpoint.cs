using Carter;
using Mapster;
using MediatR;
using Post.Application.Posts.Commands.CreatePost;

namespace Post.API.Endpoints;

public record CreatePostRequest();

public record CreatePostResponse(Guid Id);

public class CreatePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts", async (
                CreatePostRequest request,
                ISender sender) =>
            {
                var command = request.Adapt<CreatePostCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<CreatePostResponse>();
            
                // Trả về 201 Created để báo hiệu resource đã được tạo thành công
                return Results.Created($"/v1/posts/{response.Id}", response);
            })
            .RequireAuthorization(); 
    }
}