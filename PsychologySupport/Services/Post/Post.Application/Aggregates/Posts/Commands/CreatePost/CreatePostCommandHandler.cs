using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Integration;
using Microsoft.EntityFrameworkCore;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Aggregates.Posts.Commands.CreatePost;

public sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, CreatePostResult>
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionResolver _aliasResolver;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;

    public CreatePostCommandHandler(
        IPostDbContext context,
        IAliasVersionResolver aliasResolver,
        IActorResolver actorResolver,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _aliasResolver = aliasResolver;
        _actorResolver = actorResolver;
        _outboxWriter = outboxWriter;
    }

    public async Task<CreatePostResult> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var aliasVersionId = await _aliasResolver.GetCurrentAliasVersionIdAsync(cancellationToken);
        

        // Create post aggregate
        var post = Domain.Aggregates.Posts.Post.Create(
            _actorResolver.AliasId,
            request.Content,
            request.Title,
            aliasVersionId,
            request.Visibility
        );

        // Add media if provided
        if (request.MediaIds?.Any() == true)
        {
            foreach (var mediaId in request.MediaIds)
            {
                post.AddMedia(mediaId);
            }
        }

        _context.Posts.Add(post);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreatePostResult(
            post.Id,
            post.Moderation.Status.ToString(),
            post.CreatedAt
        );
    }
}
