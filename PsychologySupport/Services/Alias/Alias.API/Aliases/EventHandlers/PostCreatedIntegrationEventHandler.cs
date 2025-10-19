using Alias.API.Data.Public;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Alias.API.Aliases.Features.UpdateMetadata.IntegrationEventHandlers;

/// <summary>
/// Handles PostCreatedIntegrationEvent to update alias metadata when a post is created
/// </summary>
public class PostCreatedIntegrationEventHandler : IConsumer<PostCreatedIntegrationEvent>
{
    private readonly AliasDbContext _dbContext;
    private readonly ILogger<PostCreatedIntegrationEventHandler> _logger;

    public PostCreatedIntegrationEventHandler(
        AliasDbContext dbContext,
        ILogger<PostCreatedIntegrationEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing PostCreatedIntegrationEvent for PostId: {PostId}, AuthorAliasId: {AuthorAliasId}",
            message.PostId, message.AuthorAliasId);

        try
        {
            // Find the alias
            var alias = await _dbContext.Aliases
                .FirstOrDefaultAsync(a => a.Id == message.AuthorAliasId && !a.IsDeleted);

            if (alias == null)
            {
                _logger.LogWarning(
                    "Alias not found for PostCreatedIntegrationEvent. AliasId: {AliasId}",
                    message.AuthorAliasId);
                return;
            }

            // Increment posts count
            alias.IncrementPostsCount();

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully updated alias metadata for PostCreatedIntegrationEvent. AliasId: {AliasId}, PostId: {PostId}",
                message.AuthorAliasId, message.PostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing PostCreatedIntegrationEvent. PostId: {PostId}, AuthorAliasId: {AuthorAliasId}",
                message.PostId, message.AuthorAliasId);
            throw;
        }
    }
}
