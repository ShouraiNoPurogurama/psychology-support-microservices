using BuildingBlocks.CQRS;
using Post.Application.Data;

namespace Post.Application.Posts.Commands.CreatePost;

public class CreatePostHandler() : ICommandHandler<CreatePostCommand, CreatePostResult>
{
    
    private readonly IPublicDbContext _dbContext;
    // private readonly ICurrentUserProvider _currentUserProvider;

    public Task<CreatePostResult> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}