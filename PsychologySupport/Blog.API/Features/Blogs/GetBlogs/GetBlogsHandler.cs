using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Blog.API.Data;

namespace Blog.API.Features.Blogs.GetBlogs;

public record GetBlogsQuery(PaginationRequest Pagination) : IQuery<GetBlogsResult>;

public record GetBlogsResult(PaginatedResult<Models.Blog> Blogs);

public class GetBlogsHandler : IQueryHandler<GetBlogsQuery, GetBlogsResult>
{
    private readonly BlogDbContext _dbContext;

    public GetBlogsHandler(BlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetBlogsResult> Handle(GetBlogsQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Pagination.PageIndex - 1) * request.Pagination.PageSize;

        var totalCount = await _dbContext.Blogs.CountAsync(cancellationToken);

        var blogs = await _dbContext.Blogs
            .Skip(skip)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<Models.Blog>(
            request.Pagination.PageIndex,
            request.Pagination.PageSize,
            totalCount,
            blogs
        );

        return new GetBlogsResult(paginatedResult);
    }
}