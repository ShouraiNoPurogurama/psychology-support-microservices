using BuildingBlocks.Messaging.Events.IntegrationEvents.Chatbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using UserMemory.API.Data;
using UserMemory.API.Models;

namespace UserMemory.API.Domains.Progress.Consumers;

public class SessionCreatedIntegrationEventHandler : IConsumer<SessionCreatedIntegrationEvent>
{
    private readonly UserMemoryDbContext _dbContext;
    private readonly ILogger<MessageProcessedIntegrationEventConsumer> _logger;


    public SessionCreatedIntegrationEventHandler(
        UserMemoryDbContext dbContext,
        ILogger<MessageProcessedIntegrationEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SessionCreatedIntegrationEvent> context)
    {
        var msg = context.Message;
        var msgId = context.MessageId ?? Guid.NewGuid();

        _logger.LogInformation(
            "Processing message progress for AliasId={AliasId}, SessionId={SessionId}, MessageId={MessageId}",
            msg.AliasId, msg.SessionId, msgId);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(context.CancellationToken);

        try
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // --- UPDATE SESSION DAILY PROGRESS ---
            var sessionProgress = await _dbContext.SessionDailyProgresses
                .FirstOrDefaultAsync(p =>
                        p.AliasId == msg.AliasId &&
                        p.ProgressDate == today &&
                        p.SessionId == msg.SessionId,
                    context.CancellationToken);

            if (sessionProgress == null)
            {
                sessionProgress = new SessionDailyProgress
                {
                    AliasId = msg.AliasId,
                    SessionId = msg.SessionId,
                    ProgressDate = today,
                    ProgressPoints = 0,
                    LastIncrement = 0,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _dbContext.SessionDailyProgresses.Add(sessionProgress);

                _logger.LogInformation(
                    "Created new SessionDailyProgress for Session={SessionId}, Points={Points}",
                    msg.SessionId, 0);
            }

            // --- UPDATE OR CREATE ALIAS DAILY SUMMARY ---
            var dailySummary = await _dbContext.AliasDailySummaries
                .FirstOrDefaultAsync(s =>
                        s.AliasId == msg.AliasId &&
                        s.Date == today,
                    context.CancellationToken);

            if (dailySummary == null)
            {
                dailySummary = new AliasDailySummary
                {
                    AliasId = msg.AliasId,
                    Date = today,
                    RewardClaimCount = 0,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _dbContext.AliasDailySummaries.Add(dailySummary);
                
                _logger.LogInformation(
                    "Created new AliasDailySummary for Alias={AliasId}, TotalDailyPoints={Points}",
                    msg.AliasId, 0);
            }

            await _dbContext.SaveChangesAsync(context.CancellationToken);
            await transaction.CommitAsync(context.CancellationToken);
            
            _logger.LogInformation(
                "Published ProgressUpdatedIntegrationEvent for AliasId={AliasId}",
                msg.AliasId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process message progress for AliasId={AliasId}, SessionId={SessionId}",
                msg.AliasId, msg.SessionId);

            await transaction.RollbackAsync(context.CancellationToken);
            throw;
        }
    }

}
