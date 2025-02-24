using BuildingBlocks.CQRS;
using Blog.API.Data;
using Blog.API.Models;

namespace Blog.API.Features.Blogs.CreateBlog;

public record CreateBlogCommand(Models.Blog Blog) : ICommand<CreateBlogResult>;

public record CreateBlogResult(Guid Id);

public class CreateBlogHandler : ICommandHandler<CreateBlogCommand, CreateBlogResult>
{
    private readonly BlogDbContext _context;

    public CreateBlogHandler(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<CreateBlogResult> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
        _context.Blogs.Add(request.Blog);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateBlogResult(request.Blog.Id);
    }
}