using BuildingBlocks.Messaging.Events.IntegrationEvents.Chatbox;
using BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using UserMemory.API.Data;
using UserMemory.API.Data.Options;
using UserMemory.API.Models;
using UserMemory.API.Shared.Enums;

namespace UserMemory.API.Domains.Progress.Consumers;

/// <summary>
/// Consumes MessageProcessedIntegrationEvent from ChatBox.API.
/// Tracks daily progress points for EVERY user message, regardless of whether memory is saved.
/// Publishes ProgressUpdatedIntegrationEvent for real-time client updates via SignalR.
/// </summary>
public class MessageProcessedIntegrationEventConsumer : IConsumer<MessageProcessedIntegrationEvent>
{
    private readonly UserMemoryDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MessageProcessedIntegrationEventConsumer> _logger;

    // Point calculation constants
    private static int AnyMessagePoints = MessagePointOptions.AnyMessagePoints;
    private static int SaveNeededPoints = MessagePointOptions.SaveNeededPoints;
    private static int EmotionOrPersonalPoints = MessagePointOptions.EmotionOrPersonalPoints;

    public MessageProcessedIntegrationEventConsumer(
        UserMemoryDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ILogger<MessageProcessedIntegrationEventConsumer> logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MessageProcessedIntegrationEvent> context)
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

            // Calculate points for this message
            var pointsEarned = CalculateProgressPoints(msg.SaveNeeded, msg.Tags);

            _logger.LogInformation(
                "Points calculated: {Points} (SaveNeeded={SaveNeeded}, TagCount={TagCount})",
                pointsEarned, msg.SaveNeeded, msg.Tags?.Count ?? 0);

            // --- UPDATE SESSION DAILY PROGRESS ---
            var sessionProgress = await _dbContext.SessionDailyProgresses
                .FirstOrDefaultAsync(p =>
                        p.AliasId == msg.AliasId &&
                        p.ProgressDate == today &&
                        p.SessionId == msg.SessionId,
                    context.CancellationToken);

            int totalSessionPoints;
            if (sessionProgress == null)
            {
                // Create new session progress record
                sessionProgress = new SessionDailyProgress
                {
                    AliasId = msg.AliasId,
                    SessionId = msg.SessionId,
                    ProgressDate = today,
                    ProgressPoints = pointsEarned,
                    LastIncrement = pointsEarned,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _dbContext.SessionDailyProgresses.Add(sessionProgress);
                totalSessionPoints = pointsEarned;

                _logger.LogInformation(
                    "Created new SessionDailyProgress for Session={SessionId}, Points={Points}",
                    msg.SessionId, pointsEarned);
            }
            else
            {
                // Update existing session progress
                sessionProgress.ProgressPoints += pointsEarned;
                sessionProgress.LastIncrement = pointsEarned;
                sessionProgress.LastModified = DateTimeOffset.UtcNow;
                totalSessionPoints = sessionProgress.ProgressPoints;

                _logger.LogInformation(
                    "Updated SessionDailyProgress for Session={SessionId}, OldPoints={OldPoints}, NewPoints={NewPoints}",
                    msg.SessionId, sessionProgress.ProgressPoints - pointsEarned, sessionProgress.ProgressPoints);
            }

            // --- UPDATE OR CREATE ALIAS DAILY SUMMARY ---
            // This aggregates all sessions' points for the day
            var dailySummary = await _dbContext.AliasDailySummaries
                .FirstOrDefaultAsync(s =>
                        s.AliasId == msg.AliasId &&
                        s.Date == today,
                    context.CancellationToken);

            int totalDailyPoints;
            if (dailySummary == null)
            {
                // Create new daily summary
                dailySummary = new AliasDailySummary
                {
                    AliasId = msg.AliasId,
                    Date = today,
                    RewardClaimCount = 0,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _dbContext.AliasDailySummaries.Add(dailySummary);

                // Calculate total from all sessions for today
                totalDailyPoints = await CalculateTotalDailyPointsAsync(msg.AliasId, today, context.CancellationToken);
                totalDailyPoints += pointsEarned; // Add current points

                _logger.LogInformation(
                    "Created new AliasDailySummary for Alias={AliasId}, TotalDailyPoints={Points}",
                    msg.AliasId, totalDailyPoints);
            }
            else
            {
                // Calculate total from all sessions
                totalDailyPoints = await CalculateTotalDailyPointsAsync(msg.AliasId, today, context.CancellationToken);

                _logger.LogInformation(
                    "Updated AliasDailySummary for Alias={AliasId}, TotalDailyPoints={Points}",
                    msg.AliasId, totalDailyPoints);
            }

            // Save all changes
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            await transaction.CommitAsync(context.CancellationToken);

            _logger.LogInformation(
                "Progress tracking completed for AliasId={AliasId}, PointsEarned={PointsEarned}, TotalSession={TotalSession}, TotalDaily={TotalDaily}",
                msg.AliasId, pointsEarned, totalSessionPoints, totalDailyPoints);

            // --- PUBLISH PROGRESS UPDATED EVENT FOR REAL-TIME NOTIFICATION ---
            var progressEvent = new ProgressUpdatedIntegrationEvent(
                AliasId: msg.AliasId,
                SessionId: msg.SessionId,
                PointsEarned: pointsEarned,
                TotalSessionPoints: totalSessionPoints,
                TotalDailyPoints: totalDailyPoints,
                Achievement: DetermineAchievement(totalSessionPoints),
                UpdatedAt: DateTimeOffset.UtcNow
            );

            await _publishEndpoint.Publish(progressEvent, context.CancellationToken);

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

    /// <summary>
    /// Calculate total daily points by summing all session progress for the alias on the given date.
    /// </summary>
    private async Task<int> CalculateTotalDailyPointsAsync(Guid aliasId, DateOnly date, CancellationToken cancellationToken)
    {
        var total = await _dbContext.SessionDailyProgresses
            .Where(p => p.AliasId == aliasId && p.ProgressDate == date)
            .SumAsync(p => p.ProgressPoints, cancellationToken);

        return total;
    }

    /// <summary>
    /// Calculate progress points for a message based on SaveNeeded flag and tags.
    /// </summary>
    private static int CalculateProgressPoints(bool saveNeeded, List<string>? tags)
    {
        var sum = AnyMessagePoints; // Base points for any message

        if (saveNeeded)
            sum += SaveNeededPoints; // Bonus for memory-worthy messages

        if (tags is { Count: > 0 } && HasEmotionOrPersonalLife(tags))
            sum += EmotionOrPersonalPoints; // Bonus for emotional/personal content

        return sum;
    }

    /// <summary>
    /// Check if tags contain emotion or personal life indicators.
    /// </summary>
    private static bool HasEmotionOrPersonalLife(IEnumerable<string> tags)
    {
        foreach (var raw in tags)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var t = raw.Trim();

            // Check for emotion or personal_life prefix
            if (t.StartsWith("emotion_", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.StartsWith("personal_life_", StringComparison.OrdinalIgnoreCase)) return true;

            // Check for specific topic/relationship tags
            if (TryMapTopicTag(t, out var topic))
            {
                if (topic is TopicTag.Topic_Family or TopicTag.Topic_Health or TopicTag.Topic_Hobby or TopicTag.Topic_Travel)
                    return true;
            }

            if (TryMapRelationshipTag(t, out var rel))
            {
                if (rel != RelationshipTag.Relationship_Colleague)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determine achievement message based on total daily points.
    /// </summary>
    private static string? DetermineAchievement(int totalDailyPoints)
    {
        return totalDailyPoints switch
        {
            >= 5000 => " Bạn đã đạt 5,000 điểm hôm nay! Tuyệt vời!",
            >= 3000 => " Đạt 3,000 điểm! Bạn đang làm rất tốt!",
            >= 1000 => " Bạn đã đạt 1,000 điểm! Có thể đổi thưởng rồi!",
            >= 500 => " Đã kiếm được 500 điểm, sắp tới phần thưởng rồi!",
            _ => null
        };
    }

    private static bool TryMapTopicTag(string tag, out TopicTag value)
        => Enum.TryParse(NormalizeEnumName(tag, "topic_"), ignoreCase: true, out value);

    private static bool TryMapRelationshipTag(string tag, out RelationshipTag value)
        => Enum.TryParse(NormalizeEnumName(tag, "relationship_"), ignoreCase: true, out value);

    private static string NormalizeEnumName(string input, string knownPrefix)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input.Trim();

        if (s.Contains('_') && char.IsUpper(s[0])) return s;

        s = s.Replace('-', '_');
        if (!s.Contains('_') && !s.StartsWith(knownPrefix, StringComparison.OrdinalIgnoreCase))
            s = $"{knownPrefix}{s}";

        var parts = s.Split('_', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
        {
            var p = parts[i].ToLowerInvariant();
            parts[i] = char.ToUpperInvariant(p[0]) + p[1..];
        }

        return string.Join('_', parts);
    }
}