using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.Extensions.Logging;
using Post.Application.Data;

namespace Post.Application.Features.Posts.Commands.FinalizePostCreation;

public class FinalizePostCreationCommandHandler(
    IPostDbContext dbContext,
    ILogger<FinalizePostCreationCommandHandler> logger)
    : ICommandHandler<FinalizePostCreationCommand, FinalizePostCreationResult>
{
    public async Task<FinalizePostCreationResult> Handle(FinalizePostCreationCommand request, CancellationToken cancellationToken)
    {
        var post = await dbContext.Posts.FindAsync(new object[] { request.PostId }, cancellationToken);

        if (post is null)
        {
            //Nếu không tìm thấy post, đây là một lỗi nghiêm trọng trong luồng Saga.
            throw new NotFoundException($"Post with ID {request.PostId} not found during creation finalization.",
                "POST_NOT_FOUND");
        }

        post.FinalizeCreation();

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully finalized creation for PostId: {PostId}", request.PostId);

        return new FinalizePostCreationResult(true);
    }
}