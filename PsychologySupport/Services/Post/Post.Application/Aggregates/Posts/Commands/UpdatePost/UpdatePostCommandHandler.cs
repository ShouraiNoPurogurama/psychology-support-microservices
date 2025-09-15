using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Posts.Commands.UpdatePost;

internal sealed class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand, UpdatePostResult>
{
    private readonly IPostDbContext _context;
    private readonly IActorResolver _actorResolver;

    public UpdatePostCommandHandler(
        IPostDbContext context,
        IActorResolver actorResolver)
    {
        _context = context;
        _actorResolver = actorResolver;
    }

    public async Task<UpdatePostResult> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post is null)
        {
            throw new NotFoundException($"Post with ID {request.PostId} not found");
        }

        // The domain aggregate will validate edit permissions
        post.UpdateContent(request.NewContent, request.NewTitle, request.EditorAliasId);

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdatePostResult(
            post.Id,
            post.EditedAt!.Value
        );
    }
}
