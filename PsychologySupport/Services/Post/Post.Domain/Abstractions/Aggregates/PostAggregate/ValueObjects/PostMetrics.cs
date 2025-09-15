namespace Post.Domain.Abstractions.Aggregates.PostAggregate.ValueObjects;

public sealed record PostMetrics
{
    public int ReactionCount { get; init; }
    public int CommentCount { get; init; }
    public int ShareCount { get; init; }
    public int ViewCount { get; init; }

    public PostMetrics(int reactionCount = 0, int commentCount = 0, int shareCount = 0, int viewCount = 0)
    {
        ReactionCount = Math.Max(0, reactionCount);
        CommentCount = Math.Max(0, commentCount);
        ShareCount = Math.Max(0, shareCount);
        ViewCount = Math.Max(0, viewCount);
    }

    public PostMetrics IncrementReactions(int count = 1) => this with { ReactionCount = ReactionCount + Math.Max(0, count) };
    public PostMetrics DecrementReactions(int count = 1) => this with { ReactionCount = Math.Max(0, ReactionCount - Math.Max(0, count)) };
    
    public PostMetrics IncrementComments(int count = 1) => this with { CommentCount = CommentCount + Math.Max(0, count) };
    public PostMetrics DecrementComments(int count = 1) => this with { CommentCount = Math.Max(0, CommentCount - Math.Max(0, count)) };
    
    public PostMetrics IncrementShares(int count = 1) => this with { ShareCount = ShareCount + Math.Max(0, count) };
    public PostMetrics IncrementViews(int count = 1) => this with { ViewCount = ViewCount + Math.Max(0, count) };

    public int TotalEngagement => ReactionCount + CommentCount + ShareCount;
    public double EngagementRate => ViewCount > 0 ? (double)TotalEngagement / ViewCount : 0;
    public bool IsPopular => TotalEngagement > 100; // Business rule for popular posts
    public bool IsTrending => EngagementRate > 0.1; // Business rule for trending posts
}
