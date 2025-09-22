using Feed.Application.Abstractions.RankingService;
using Feed.Application.Configuration;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Feed.Application.Features.Ranking;

public sealed class UpdatePostRankCommandConsumer(
    IRankingService rankingService,
    IOptions<FeedConfiguration> options,
    ILogger<UpdatePostRankCommandConsumer> logger)
    : IConsumer<UpdatePostRankCommand>
{
    private readonly FeedConfiguration _config = options.Value;

    public async Task Consume(ConsumeContext<UpdatePostRankCommand> context)
    {
        var postId = context.Message.PostId;
        var rank = await rankingService.GetPostRankAsync(postId, context.CancellationToken);
        if (rank is null)
        {
            logger.LogDebug("Rank data missing for post {PostId}, initializing.", postId);
            await rankingService.InitializePostRankAsync(postId, DateTimeOffset.UtcNow, context.CancellationToken);
            rank = await rankingService.GetPostRankAsync(postId, context.CancellationToken);
            if (rank is null) return;
        }

        var engagement = Math.Max(0, rank.Reactions) + Math.Max(0, rank.Comments);
        var hours = Math.Max(0.0, (DateTimeOffset.UtcNow - rank.CreatedAt).TotalHours);
        var decay = _config.RankDecayFactor <= 0 ? 24.0 : _config.RankDecayFactor;
        var score = Math.Log(1 + engagement) - (hours / decay);

        var updated = new PostRankData(
            Score: score,
            Reactions: rank.Reactions,
            Comments: rank.Comments,
            Ctr: rank.Ctr,
            UpdatedAt: DateTimeOffset.UtcNow,
            CreatedAt: rank.CreatedAt,
            AuthorAliasId: rank.AuthorAliasId
        );

        await rankingService.UpdatePostRankAsync(postId, updated, context.CancellationToken);

        // Optionally push to today's trending set
        await rankingService.AddToTrendingAsync(postId, score, DateTime.UtcNow, context.CancellationToken);

        logger.LogDebug("Updated rank for post {PostId}: score={Score}", postId, score);
    }
}

