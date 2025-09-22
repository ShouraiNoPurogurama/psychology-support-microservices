using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;

namespace Post.Application.Features.Posts.Commands.UpdatePost;

internal sealed class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand, UpdatePostResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;

    public UpdatePostCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
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
