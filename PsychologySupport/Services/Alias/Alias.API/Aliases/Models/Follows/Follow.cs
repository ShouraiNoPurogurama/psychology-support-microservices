using Alias.API.Aliases.Exceptions.DomainExceptions;
 using Alias.API.Aliases.Models.Aliases.DomainEvents;
 using BuildingBlocks.DDD;

 namespace Alias.API.Aliases.Models.Follows;

/// <summary>
/// Represents a follow relationship between two aliases.
/// This is an aggregate root that manages the social connection between a follower and followed alias.
/// </summary>
public sealed class Follow : AggregateRoot<Guid>
{
    /// <summary>
    /// The ID of the alias that is following another alias.
    /// </summary>
    public Guid FollowerAliasId { get; private set; }

    /// <summary>
    /// The ID of the alias that is being followed.
    /// </summary>
    public Guid FollowedAliasId { get; private set; }

    /// <summary>
    /// The timestamp when the follow relationship was established.
    /// </summary>
    public DateTimeOffset FollowedAt { get; private set; }

    /// <summary>
    /// Private constructor for EF Core materialization.
    /// </summary>
    private Follow()
    {
    }

    /// <summary>
    /// Factory method to create a new follow relationship.
    /// </summary>
    /// <param name="followerAliasId">The ID of the alias that wants to follow</param>
    /// <param name="followedAliasId">The ID of the alias to be followed</param>
    /// <returns>A new Follow aggregate instance</returns>
    /// <exception cref="InvalidAliasDataException">Thrown when business rules are violated</exception>
    public static Follow Create(Guid followerAliasId, Guid followedAliasId)
    {
        // Business rule: An alias cannot follow itself
        if (followerAliasId == followedAliasId)
            throw new InvalidAliasDataException("An alias cannot follow itself.");

        // Business rule: Both IDs must be valid
        if (followerAliasId == Guid.Empty)
            throw new InvalidAliasDataException("Follower alias ID cannot be empty.");

        if (followedAliasId == Guid.Empty)
            throw new InvalidAliasDataException("Followed alias ID cannot be empty.");

        var follow = new Follow
        {
            Id = Guid.NewGuid(),
            FollowerAliasId = followerAliasId,
            FollowedAliasId = followedAliasId,
            FollowedAt = DateTimeOffset.UtcNow
        };

        // Publish domain event for follow creation
        follow.AddDomainEvent(new FollowCreatedDomainEvent(
            follow.Id,
            followerAliasId,
            followedAliasId,
            follow.FollowedAt));

        return follow;
    }

    /// <summary>
    /// Removes the follow relationship and publishes appropriate domain events.
    /// </summary>
    public void Remove()
    {
        // Publish domain event for follow removal
        AddDomainEvent(new FollowRemovedDomainEvent(
            Id,
            FollowerAliasId,
            FollowedAliasId,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Determines if this follow relationship matches the given alias IDs.
    /// </summary>
    /// <param name="followerAliasId">The follower alias ID to check</param>
    /// <param name="followedAliasId">The followed alias ID to check</param>
    /// <returns>True if this follow relationship matches the given IDs</returns>
    public bool IsMatch(Guid followerAliasId, Guid followedAliasId)
    {
        return FollowerAliasId == followerAliasId && FollowedAliasId == followedAliasId;
    }
}
