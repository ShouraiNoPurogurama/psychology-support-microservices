using BuildingBlocks.CQRS;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;

namespace Post.Application.Features.Posts.Commands.CreatePost;

public sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, CreatePostResult>
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionAccessor _aliasAccessor;
    private readonly ICurrentActorAccessor _currentActorAccessor;

    public CreatePostCommandHandler(
        IPostDbContext context,
        IAliasVersionAccessor aliasAccessor,
        ICurrentActorAccessor currentActorAccessor)
    {
        _context = context;
        _aliasAccessor = aliasAccessor;
        _currentActorAccessor = currentActorAccessor;
    }

    public async Task<CreatePostResult> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var aliasVersionId = await _aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(cancellationToken);
        
        // Create post aggregate
        var post = Domain.Aggregates.Posts.Post.Create(
            _currentActorAccessor.GetRequiredAliasId(),
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
