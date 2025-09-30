namespace Feed.Application.Abstractions.FanOut;

/// <summary>
/// Service for fan-out operations to distribute posts to followers' feeds.
/// </summary>
public interface IFeedFanOutService
{
    /// <summary>
    /// Fan out a published post to all followers of the author.
    /// </summary>
    Task FanOutPostAsync(
        Guid postId,
        Guid authorAliasId,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken);

    /// <summary>
    /// Remove a post from all followers' feeds (for deleted/unpublished posts).
    /// </summary>
    Task RemovePostFromFeedsAsync(
        Guid postId,
        Guid authorAliasId,
        CancellationToken cancellationToken);
}
