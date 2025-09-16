namespace Post.Domain.Aggregates.Posts.ValueObjects;

public sealed record PostMetrics
{
    public int ReactionCount { get; init; }
    public int CommentCount  { get; init; }
    public int ShareCount    { get; init; }
    public int ViewCount     { get; init; }

    // EF Core materialization
    private PostMetrics() { }

    private PostMetrics(int reactions, int comments, int shares, int views)
    {
        ReactionCount = Math.Max(0, reactions);
        CommentCount  = Math.Max(0, comments);
        ShareCount    = Math.Max(0, shares);
        ViewCount     = Math.Max(0, views);
    }

    public static PostMetrics Create(int reactionCount = 0, int commentCount = 0, int shareCount = 0, int viewCount = 0)
        => new(reactionCount, commentCount, shareCount, viewCount);

    public PostMetrics IncrementReactions(int count = 1) => this with { ReactionCount = ReactionCount + Math.Max(0, count) };
    public PostMetrics DecrementReactions(int count = 1) => this with { ReactionCount = Math.Max(0, ReactionCount - Math.Max(0, count)) };

    public PostMetrics IncrementComments(int count = 1) => this with { CommentCount = CommentCount + Math.Max(0, count) };
    public PostMetrics DecrementComments(int count = 1) => this with { CommentCount = Math.Max(0, CommentCount - Math.Max(0, count)) };

    public PostMetrics IncrementShares(int count = 1) => this with { ShareCount = ShareCount + Math.Max(0, count) };
    public PostMetrics IncrementViews(int count = 1)  => this with { ViewCount  = ViewCount  + Math.Max(0, count) };

    public int TotalEngagement   => ReactionCount + CommentCount + ShareCount;
    public double EngagementRate => ViewCount > 0 ? (double)TotalEngagement / ViewCount : 0;

    public bool IsPopular  => TotalEngagement > 100;
    public bool IsTrending => EngagementRate > 0.1;
}