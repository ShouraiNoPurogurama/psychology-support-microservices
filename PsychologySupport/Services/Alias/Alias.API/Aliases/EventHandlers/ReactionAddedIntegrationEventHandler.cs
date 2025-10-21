using Alias.API.Data.Public;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;

namespace Alias.API.Aliases.EventHandlers;

/// <summary>
/// Handles ReactionAddedIntegrationEvent to update alias metadata when a reaction is added
/// Updates both the reactor (reaction given) and target author (reaction received)
/// </summary>
public class ReactionAddedIntegrationEventHandler : IConsumer<ReactionAddedIntegrationEvent>
{
    private readonly AliasDbContext _dbContext;
    private readonly ILogger<ReactionAddedIntegrationEventHandler> _logger;

    public ReactionAddedIntegrationEventHandler(
        AliasDbContext dbContext,
        ILogger<ReactionAddedIntegrationEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReactionAddedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing ReactionAddedIntegrationEvent for ReactionId: {ReactionId}, Reactor: {ReactorAliasId}, Target: {TargetAuthorAliasId}",
            message.ReactionId, message.ReactorAliasId, message.TargetAuthorAliasId);

        try
        {
            // Find both the reactor and target author aliases
            var aliasIds = new[] { message.ReactorAliasId, message.TargetAuthorAliasId };
            var aliases = await _dbContext.Aliases
                .Where(a => aliasIds.Contains(a.Id) && !a.IsDeleted)
                .ToListAsync();

            // Update reactor - increment reactions given count
            var reactorAlias = aliases.FirstOrDefault(a => a.Id == message.ReactorAliasId);
            if (reactorAlias != null)
            {
                reactorAlias.IncrementReactionsGivenCount();
                _logger.LogInformation(
                    "Incremented reactions given count for reactor alias: {AliasId}",
                    message.ReactorAliasId);
            }
            else
            {
                _logger.LogWarning(
                    "Reactor alias not found. AliasId: {AliasId}",
                    message.ReactorAliasId);
            }

            // Update target author - increment reactions received count (only if different from reactor)
            if (message.ReactorAliasId != message.TargetAuthorAliasId)
            {
                var targetAlias = aliases.FirstOrDefault(a => a.Id == message.TargetAuthorAliasId);
                if (targetAlias != null)
                {
                    targetAlias.IncrementReactionsReceivedCount();
                    _logger.LogInformation(
                        "Incremented reactions received count for target alias: {AliasId}",
                        message.TargetAuthorAliasId);
                }
                else
                {
                    _logger.LogWarning(
                        "Target author alias not found. AliasId: {AliasId}",
                        message.TargetAuthorAliasId);
                }
            }

            if (aliases.Any())
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation(
                    "Successfully updated alias metadata for ReactionAddedIntegrationEvent. ReactionId: {ReactionId}",
                    message.ReactionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing ReactionAddedIntegrationEvent. ReactionId: {ReactionId}, Reactor: {ReactorAliasId}",
                message.ReactionId, message.ReactorAliasId);
            throw;
        }
    }
}
