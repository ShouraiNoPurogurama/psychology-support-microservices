using BuildingBlocks.CQRS;
using FluentValidation;
using Blog.API.Data;
using Blog.API.Exceptions;

namespace Blog.API.Features.Blogs.GetBlog;

public record GetBlogQuery(Guid Id) : IQuery<GetBlogResult>;

public record GetBlogResult(Models.Blog Blog);

public class GetBlogQueryValidator : AbstractValidator<GetBlogQuery>
{
    public GetBlogQueryValidator()
    {
        RuleFor(q => q.Id).NotEmpty().WithMessage(" ID cannot be null or empty");
    }
}

public class GetBlogHandler : IQueryHandler<GetBlogQuery, GetBlogResult>
{
    private readonly BlogDbContext _context;

    public GetBlogHandler(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<GetBlogResult> Handle(GetBlogQuery query, CancellationToken cancellationToken)
    {
        var blog = await _context.Blogs.FindAsync(query.Id)
                   ?? throw new BlogNotFoundException(query.Id.ToString());

        return new GetBlogResult(blog);
    }
}