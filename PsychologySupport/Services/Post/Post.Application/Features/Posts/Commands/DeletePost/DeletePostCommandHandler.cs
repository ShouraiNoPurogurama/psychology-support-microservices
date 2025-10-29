using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Posts.DomainEvents;

namespace Post.Application.Features.Posts.Commands.DeletePost;

internal sealed class DeletePostCommandHandler : ICommandHandler<DeletePostCommand, DeletePostResult>
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionAccessor _aliasAccessor;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public DeletePostCommandHandler(
        IPostDbContext context,
        IAliasVersionAccessor aliasAccessor,
        IOutboxWriter outboxWriter, ICurrentActorAccessor currentActorAccessor)
    {
        _context = context;
        _aliasAccessor = aliasAccessor;
        _outboxWriter = outboxWriter;
        _currentActorAccessor = currentActorAccessor;
    }

    public async Task<DeletePostResult> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (post is null)
        {
            throw new NotFoundException($"Post with ID {request.PostId} not found");
        }

        // Verify ownership
        if (post.Author.AliasId != _currentActorAccessor.GetRequiredAliasId())
        {
            throw new UnauthorizedAccessException("Bạn chỉ có thể xóa bài viết của chính mình.");
        }

        // Soft delete the post
        post.Delete(_currentActorAccessor.GetRequiredAliasId());

        // Add domain event
        var postDeletedEvent = new PostDeletedEvent(post.Id, _currentActorAccessor.GetRequiredAliasId());
        await _outboxWriter.WriteAsync(postDeletedEvent, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new DeletePostResult(post.Id, post.DeletedAt!.Value);
    }
}
