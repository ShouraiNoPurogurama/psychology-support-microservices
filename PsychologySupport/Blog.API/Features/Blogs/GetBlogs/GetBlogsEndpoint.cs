using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Pagination;

namespace Blog.API.Features.Blogs.GetBlogs;

public record GetBlogsRequest(int PageIndex, int PageSize);

public record GetBlogsResponse(PaginatedResult<Models.Blog> Blogs);

public class GetBlogsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("blogs", async ([FromQuery] int pageIndex, [FromQuery] int pageSize, ISender sender) =>
        {
            var query = new GetBlogsQuery(new PaginationRequest(pageIndex, pageSize));
            var result = await sender.Send(query);

            var response = result.Adapt<GetBlogsResponse>();
            return Results.Ok(response);
        })
            .WithName("GetBlogs")
            .Produces<GetBlogsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Blogs")
            .WithSummary("Get Blogs");
    }
}