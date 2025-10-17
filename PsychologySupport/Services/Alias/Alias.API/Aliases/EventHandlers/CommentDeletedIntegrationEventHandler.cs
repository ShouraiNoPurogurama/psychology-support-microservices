using Alias.API.Data.Public;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Alias.API.Aliases.Features.UpdateMetadata.IntegrationEventHandlers;

/// <summary>
/// Handles CommentDeletedIntegrationEvent to update alias metadata when a comment is deleted
/// Decrements the comment author's comments count
/// </summary>
public class CommentDeletedIntegrationEventHandler : IConsumer<CommentDeletedIntegrationEvent>
{
    private readonly AliasDbContext _dbContext;
    private readonly ILogger<CommentDeletedIntegrationEventHandler> _logger;

    public CommentDeletedIntegrationEventHandler(
        AliasDbContext dbContext,
        ILogger<CommentDeletedIntegrationEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CommentDeletedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing CommentDeletedIntegrationEvent for CommentId: {CommentId}, DeletedBy: {DeletedByAliasId}",
            message.CommentId, message.DeletedByAliasId);

        try
        {
            // Find the alias who deleted the comment (assuming it's their own comment)
            var alias = await _dbContext.Aliases
                .FirstOrDefaultAsync(a => a.Id == message.DeletedByAliasId && !a.IsDeleted);

            if (alias == null)
            {
                _logger.LogWarning(
                    "Alias not found for CommentDeletedIntegrationEvent. AliasId: {AliasId}",
                    message.DeletedByAliasId);
                return;
            }

            // Decrement comments count
            alias.DecrementCommentsCount();

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully updated alias metadata for CommentDeletedIntegrationEvent. AliasId: {AliasId}, CommentId: {CommentId}",
                message.DeletedByAliasId, message.CommentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing CommentDeletedIntegrationEvent. CommentId: {CommentId}, DeletedBy: {DeletedByAliasId}",
                message.CommentId, message.DeletedByAliasId);
            throw;
        }
    }
}
