using BuildingBlocks.CQRS;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;

namespace Post.Application.Posts.Commands.CreatePost;

using Post = Domain.Posts.Post;

public class CreatePostHandler(
    IPublicDbContext dbContext,
    IActorResolver actorResolver,        
    IAliasContextResolver aliasResolver) 
    : ICommandHandler<CreatePostCommand, CreatePostResult>
{

    public async Task<CreatePostResult> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var authorAliasId = actorResolver.AliasId;

        var authorVersionId = await aliasResolver.GetCurrentAliasVersionIdAsync(cancellationToken);

        var post = Post.Create(
            authorAliasId,
            authorVersionId,
            request.Content,
            null, 
            request.Visibility);

        dbContext.Posts.Add(post);

        await dbContext.SaveChangesAsync(cancellationToken);

        var createdAt = post.CreatedAt ?? DateTimeOffset.UtcNow;
        
        return new CreatePostResult(
            post.Id,
            post.ModerationStatus.ToString(),
            createdAt); 
    }
}