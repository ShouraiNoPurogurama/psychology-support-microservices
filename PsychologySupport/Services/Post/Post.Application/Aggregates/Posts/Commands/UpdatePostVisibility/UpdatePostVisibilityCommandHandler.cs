using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Posts.Commands.UpdatePostVisibility;

internal sealed class UpdatePostVisibilityCommandHandler : ICommandHandler<UpdatePostVisibilityCommand, UpdatePostVisibilityResult>
{
    private readonly IPostDbContext _context;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public UpdatePostVisibilityCommandHandler(
        IPostDbContext context,
        IActorResolver actorResolver,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _actorResolver = actorResolver;
        _outboxWriter = outboxWriter;
    }

    public async Task<UpdatePostVisibilityResult> Handle(UpdatePostVisibilityCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        var oldVisibility = post.Visibility;
        
        // Change visibility using existing domain method
        post.ChangeVisibility(request.Visibility, _actorResolver.AliasId);

        await _context.SaveChangesAsync(cancellationToken);

        // Publish integration event via Outbox
        await _outboxWriter.WriteAsync(
            new PostVisibilityUpdatedIntegrationEvent(
                post.Id,
                post.Author.AliasId,
                oldVisibility.ToString(),
                request.Visibility.ToString(),
                DateTimeOffset.UtcNow
            ),
            cancellationToken);

        return new UpdatePostVisibilityResult(
            post.Id,
            post.Visibility.ToString(),
            DateTimeOffset.UtcNow
        );
    }
}
