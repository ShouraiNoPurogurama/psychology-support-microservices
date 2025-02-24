using BuildingBlocks.CQRS;
using Blog.API.Data;
using Blog.API.Exceptions;

namespace Blog.API.Features.Blogs.DeleteBlog;

public record DeleteBlogCommand(Guid Id) : ICommand<DeleteBlogResult>;

public record DeleteBlogResult(bool IsSuccess);

public class DeleteBlogHandler : ICommandHandler<DeleteBlogCommand, DeleteBlogResult>
{
    private readonly BlogDbContext _context;

    public DeleteBlogHandler(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteBlogResult> Handle(DeleteBlogCommand request, CancellationToken cancellationToken)
    {
        var existingBlog = await _context.Blogs.FindAsync(request.Id)
                          ?? throw new BlogNotFoundException("Blog", request.Id);

        _context.Blogs.Remove(existingBlog);

        var result = await _context.SaveChangesAsync() > 0;

        return new DeleteBlogResult(result);
    }
}
