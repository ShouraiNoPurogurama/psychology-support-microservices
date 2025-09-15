using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Post.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Posts.Commands.DeletePost;

internal sealed class DeletePostCommandHandler : ICommandHandler<DeletePostCommand, DeletePostResult>
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionResolver _aliasResolver;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public DeletePostCommandHandler(
        IPostDbContext context,
        IAliasVersionResolver aliasResolver,
        IOutboxWriter outboxWriter, IActorResolver actorResolver)
    {
        _context = context;
        _aliasResolver = aliasResolver;
        _outboxWriter = outboxWriter;
        _actorResolver = actorResolver;
    }

    public async Task<DeletePostResult> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var aliasVersion = await _aliasResolver.GetCurrentAliasVersionIdAsync(cancellationToken);
        
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post is null)
        {
            throw new NotFoundException($"Post with ID {request.PostId} not found");
        }

        // Verify ownership
        if (post.Author.AliasId != _actorResolver.AliasId)
        {
            throw new UnauthorizedAccessException("You can only delete your own posts");
        }

        // Soft delete the post
        post.Delete(_actorResolver.AliasId);

        // Add domain event
        var postDeletedEvent = new PostDeletedEvent(post.Id, _actorResolver.AliasId);
        await _outboxWriter.WriteAsync(postDeletedEvent, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new DeletePostResult(post.Id, post.DeletedAt!.Value);
    }
}
