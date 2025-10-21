using Alias.API.Data.Public;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;

namespace Alias.API.Aliases.EventHandlers;

/// <summary>
/// Handles PostDeletedIntegrationEvent to update alias metadata when a post is deleted
/// </summary>
public class PostDeletedIntegrationEventHandler : IConsumer<PostDeletedIntegrationEvent>
{
    private readonly AliasDbContext _dbContext;
    private readonly ILogger<PostDeletedIntegrationEventHandler> _logger;

    public PostDeletedIntegrationEventHandler(
        AliasDbContext dbContext,
        ILogger<PostDeletedIntegrationEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostDeletedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing PostDeletedIntegrationEvent for PostId: {PostId}, DeletedByAliasId: {DeletedByAliasId}",
            message.PostId, message.DeletedByAliasId);

        try
        {
            // Find the alias who deleted the post (assuming it's their own post)
            var alias = await _dbContext.Aliases
                .FirstOrDefaultAsync(a => a.Id == message.DeletedByAliasId && !a.IsDeleted);

            if (alias == null)
            {
                _logger.LogWarning(
                    "Alias not found for PostDeletedIntegrationEvent. AliasId: {AliasId}",
                    message.DeletedByAliasId);
                return;
            }

            // Decrement posts count
            alias.DecrementPostsCount();

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully updated alias metadata for PostDeletedIntegrationEvent. AliasId: {AliasId}, PostId: {PostId}",
                message.DeletedByAliasId, message.PostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing PostDeletedIntegrationEvent. PostId: {PostId}, DeletedByAliasId: {DeletedByAliasId}",
                message.PostId, message.DeletedByAliasId);
            throw;
        }
    }
}
