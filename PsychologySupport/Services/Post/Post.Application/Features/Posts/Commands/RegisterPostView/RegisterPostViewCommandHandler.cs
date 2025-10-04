using BuildingBlocks.CQRS;
using Post.Application.Data;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Logging;

namespace Post.Application.Features.Posts.Commands.RegisterPostView;

public sealed class RegisterPostViewCommandHandler : ICommandHandler<RegisterPostViewCommand, RegisterPostViewResult>
{
    private readonly IPostDbContext _context;
    private readonly ILogger<RegisterPostViewCommandHandler> _logger;

    public RegisterPostViewCommandHandler(IPostDbContext context, ILogger<RegisterPostViewCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RegisterPostViewResult> Handle(RegisterPostViewCommand request, CancellationToken cancellationToken)
    {
        if (request.PostIds is null || !request.PostIds.Any())
        {
            return new RegisterPostViewResult(0, DateTimeOffset.UtcNow);
        }

        var postsToUpdate = await _context.Posts
            .Where(p => request.PostIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (!postsToUpdate.Any())
        {
            _logger.LogWarning("No valid posts found for the given IDs: {PostIds}", string.Join(", ", request.PostIds));
            return new RegisterPostViewResult(0, DateTimeOffset.UtcNow);
        }

        foreach (var post in postsToUpdate)
        {
            post.RecordView();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterPostViewResult(
            postsToUpdate.Count,
            DateTimeOffset.UtcNow
        );
    }
}