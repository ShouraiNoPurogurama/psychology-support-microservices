namespace Feed.Domain.PostReplica;

/// <summary>
/// Domain model representing post engagement counters.
/// Uses counter columns for tracking various engagement metrics.
/// </summary>
public sealed class PostEngagementCounters
{
    public Guid PostId { get; }
    public long Reactions { get; }
    public long Comments { get; }
    public long Shares { get; }
    public long Clicks { get; }
    public long Impressions { get; }
    public long ViewDurationSec { get; }

    private PostEngagementCounters(
        Guid postId,
        long reactions,
        long comments,
        long shares,
        long clicks,
        long impressions,
        long viewDurationSec)
    {
        if (postId == Guid.Empty)
            throw new ArgumentException("Thông tin bài viết không hợp lệ.", nameof(postId));

        PostId = postId;
        Reactions = reactions;
        Comments = comments;
        Shares = shares;
        Clicks = clicks;
        Impressions = impressions;
        ViewDurationSec = viewDurationSec;
    }

    public static PostEngagementCounters Create(
        Guid postId,
        long reactions = 0,
        long comments = 0,
        long shares = 0,
        long clicks = 0,
        long impressions = 0,
        long viewDurationSec = 0)
    {
        return new(
            postId,
            reactions,
            comments,
            shares,
            clicks,
            impressions,
            viewDurationSec);
    }

    public PostEngagementCounters WithIncrementedReactions(long increment = 1)
        => new(PostId, Reactions + increment, Comments, Shares, Clicks, Impressions, ViewDurationSec);

    public PostEngagementCounters WithIncrementedComments(long increment = 1)
        => new(PostId, Reactions, Comments + increment, Shares, Clicks, Impressions, ViewDurationSec);

    public PostEngagementCounters WithIncrementedShares(long increment = 1)
        => new(PostId, Reactions, Comments, Shares + increment, Clicks, Impressions, ViewDurationSec);

    public PostEngagementCounters WithIncrementedClicks(long increment = 1)
        => new(PostId, Reactions, Comments, Shares, Clicks + increment, Impressions, ViewDurationSec);

    public PostEngagementCounters WithIncrementedImpressions(long increment = 1)
        => new(PostId, Reactions, Comments, Shares, Clicks, Impressions + increment, ViewDurationSec);

    public PostEngagementCounters WithIncrementedViewDuration(long seconds)
        => new(PostId, Reactions, Comments, Shares, Clicks, Impressions, ViewDurationSec + seconds);
}
