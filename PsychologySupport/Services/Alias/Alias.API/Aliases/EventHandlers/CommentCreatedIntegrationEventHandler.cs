using Alias.API.Data.Public;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;

namespace Alias.API.Aliases.EventHandlers;

/// <summary>
/// Handles CommentCreatedIntegrationEvent to update alias metadata when a comment is created
/// Updates the comment author's comments count
/// </summary>
public class CommentCreatedIntegrationEventHandler : IConsumer<CommentCreatedIntegrationEvent>
{
    private readonly AliasDbContext _dbContext;
    private readonly ILogger<CommentCreatedIntegrationEventHandler> _logger;
 
    public CommentCreatedIntegrationEventHandler(
        AliasDbContext dbContext,
        ILogger<CommentCreatedIntegrationEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CommentCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing CommentCreatedIntegrationEvent for CommentId: {CommentId}, Author: {CommentAuthorAliasId}",
            message.CommentId, message.CommentAuthorAliasId);

        try
        {
            // Find the comment author alias
            var alias = await _dbContext.Aliases
                .FirstOrDefaultAsync(a => a.Id == message.CommentAuthorAliasId && !a.IsDeleted);

            if (alias == null)
            {
                _logger.LogWarning(
                    "Alias not found for CommentCreatedIntegrationEvent. AliasId: {AliasId}",
                    message.CommentAuthorAliasId);
                return;
            }

            // Increment comments count
            alias.IncrementCommentsCount();

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully updated alias metadata for CommentCreatedIntegrationEvent. AliasId: {AliasId}, CommentId: {CommentId}",
                message.CommentAuthorAliasId, message.CommentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing CommentCreatedIntegrationEvent. CommentId: {CommentId}, Author: {CommentAuthorAliasId}",
                message.CommentId, message.CommentAuthorAliasId);
            throw;
        }
    }
}
