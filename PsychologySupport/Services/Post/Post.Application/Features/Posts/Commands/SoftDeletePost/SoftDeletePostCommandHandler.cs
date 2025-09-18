using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;

namespace Post.Application.Features.Posts.Commands.SoftDeletePost;

internal sealed class SoftDeletePostCommandHandler : ICommandHandler<SoftDeletePostCommand, SoftDeletePostResult>
{
    private readonly IPostDbContext _context;

    public SoftDeletePostCommandHandler(IPostDbContext context)
    {
        _context = context;
    }

    public async Task<SoftDeletePostResult> Handle(SoftDeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post is null)
        {
            throw new NotFoundException($"Post with ID {request.PostId} not found");
        }

        // The domain aggregate will validate delete permissions
        post.SoftDelete(request.DeleterAliasId);

        await _context.SaveChangesAsync(cancellationToken);

        return new SoftDeletePostResult(
            post.Id,
            post.DeletedAt!.Value
        );
    }
}
