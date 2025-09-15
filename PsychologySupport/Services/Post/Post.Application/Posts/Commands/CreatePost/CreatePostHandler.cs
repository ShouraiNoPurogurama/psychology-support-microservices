using BuildingBlocks.CQRS;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;

namespace Post.Application.Posts.Commands.CreatePost;

using Post = Domain.Aggregates.Post.Post;

public class CreatePostHandler(
    IPostDbContext dbContext,
    IActorResolver actorResolver,        
    IAliasContextResolver aliasResolver) 
    : ICommandHandler<CreatePostCommand, CreatePostResult>
{

    public async Task<CreatePostResult> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var authorAliasId = actorResolver.AliasId;

        var authorVersionId = await aliasResolver.GetCurrentAliasVersionIdAsync(cancellationToken);
        
        throw new NotImplementedException();
    }
}