using BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using UserMemory.API.Data;
using UserMemory.API.Models;

namespace UserMemory.API.Domains.Rewards.Consumers;

/// <summary>
/// Saga Compensation Consumer: Handles rollback when reward processing fails.
/// Compensating actions:
/// 1. Refund points to SessionDailyProgress
/// 2. Decrement RewardClaimCount in AliasDailySummary (if applicable)
/// </summary>
public class RewardFailedIntegrationEventConsumer : IConsumer<RewardFailedIntegrationEvent>
{
    private readonly UserMemoryDbContext _dbContext;
    private readonly ILogger<RewardFailedIntegrationEventConsumer> _logger;

    public RewardFailedIntegrationEventConsumer(
        UserMemoryDbContext dbContext,
        ILogger<RewardFailedIntegrationEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RewardFailedIntegrationEvent> context)
    {
        var msg = context.Message;
        var msgId = context.MessageId ?? Guid.NewGuid();

        _logger.LogWarning(
            "Processing RewardFailed compensation for RewardId={RewardId}, AliasId={AliasId}, SessionId={SessionId}, PointsToRefund={Points}. Error: {Error}",
            msg.RewardId, msg.AliasId, msg.SessionId, msg.PointsToRefund, msg.ErrorMessage);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(context.CancellationToken);

        try
        {
            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // --- COMPENSATION ACTION 1: Refund Points to SessionDailyProgress ---
            var sessionProgress = await _dbContext.SessionDailyProgresses
                .FirstOrDefaultAsync(p =>
                    p.AliasId == msg.AliasId &&
                    p.SessionId == msg.SessionId &&
                    p.ProgressDate == currentDate,
                    context.CancellationToken);

            if (sessionProgress != null)
            {
                // Add back the points
                sessionProgress.ProgressPoints += msg.PointsToRefund;
                sessionProgress.LastModified = DateTimeOffset.UtcNow;

                _logger.LogInformation(
                    "Refunded {Points} points to SessionDailyProgress for Session={SessionId}, NewBalance={NewBalance}",
                    msg.PointsToRefund, msg.SessionId, sessionProgress.ProgressPoints);
            }
            else
            {
                // Session progress record doesn't exist (edge case) - create it with refund points
                _logger.LogWarning(
                    "SessionDailyProgress not found for Session={SessionId}. Creating new record with refunded points.",
                    msg.SessionId);

                var newProgress = new SessionDailyProgress
                {
                    AliasId = msg.AliasId,
                    SessionId = msg.SessionId,
                    ProgressDate = currentDate,
                    ProgressPoints = msg.PointsToRefund,
                    LastIncrement = 0,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _dbContext.SessionDailyProgresses.Add(newProgress);
            }

            // --- COMPENSATION ACTION 2: Decrement RewardClaimCount ---
            // Only decrement if the reward claim was counted (i.e., the record exists and count > 0)
            var dailySummary = await _dbContext.AliasDailySummaries
                .FirstOrDefaultAsync(s =>
                    s.AliasId == msg.AliasId &&
                    s.Date == currentDate,
                    context.CancellationToken);

            if (dailySummary != null && dailySummary.RewardClaimCount > 0)
            {
                dailySummary.RewardClaimCount--;
                dailySummary.LastModified = DateTimeOffset.UtcNow;

                _logger.LogInformation(
                    "Decremented RewardClaimCount for AliasId={AliasId}, NewCount={NewCount}",
                    msg.AliasId, dailySummary.RewardClaimCount);
            }
            else
            {
                _logger.LogInformation(
                    "No AliasDailySummary found or RewardClaimCount is already 0 for AliasId={AliasId}. Skipping decrement.",
                    msg.AliasId);
            }

            // --- OPTIONAL: Update Reward status if not already marked as Failed ---
            // This is defensive - the ProcessRewardRequestJob already sets it to Failed,
            // but we ensure idempotency
            var reward = await _dbContext.Rewards.FindAsync(new object[] { msg.RewardId }, context.CancellationToken);
            if (reward != null && reward.Status != UserMemory.API.Shared.Enums.RewardStatus.Failed)
            {
                reward.Status = UserMemory.API.Shared.Enums.RewardStatus.Failed;
                _logger.LogInformation("Updated Reward status to Failed for RewardId={RewardId}", msg.RewardId);
            }

            // Save all changes
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            await transaction.CommitAsync(context.CancellationToken);

            _logger.LogInformation(
                "Successfully completed compensation for RewardId={RewardId}",
                msg.RewardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to execute compensation for RewardId={RewardId}. Manual intervention may be required.",
                msg.RewardId);

            // Rollback the transaction
            await transaction.RollbackAsync(context.CancellationToken);

            // Re-throw to let MassTransit handle retry logic
            throw;
        }
    }
}
