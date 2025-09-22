using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Queries.GetPostCreationStatus;

public class GetPostCreationStatusQueryHandler(IPostDbContext dbContext)
    : IQueryHandler<GetPostCreationStatusQuery, PostCreationStatusResult>
{
    public async Task<PostCreationStatusResult> Handle(GetPostCreationStatusQuery request, CancellationToken cancellationToken)
    {
        // Sử dụng projection (.Select) để chỉ query các cột cần thiết, giúp tăng hiệu năng.
        var result = await dbContext.Posts
            .Where(p => p.Id == request.PostId)
            .Select(p => new
            {
                p.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw new NotFoundException($"Post with ID {request.PostId} not found.", "POST_NOT_FOUND");
        }
        
        string? failureReason = result.Status == PostStatus.CreationFailed ? "Media attachment failed." : null;

        return new PostCreationStatusResult(
            request.PostId,
            result.Status.ToString(),
            failureReason 
        );
    }
}