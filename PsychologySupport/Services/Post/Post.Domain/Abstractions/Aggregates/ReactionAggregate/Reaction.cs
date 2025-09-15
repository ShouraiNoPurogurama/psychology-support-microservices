using Post.Domain.Events;
using Post.Domain.Exceptions;
using Post.Domain.Abstractions.Aggregates.ReactionAggregate.ValueObjects;
using Post.Domain.Abstractions.Aggregates.PostAggregate.ValueObjects;

namespace Post.Domain.Abstractions.Aggregates.ReactionAggregate;

public sealed class Reaction : AggregateRoot<Guid>, ISoftDeletable
{
    // Value Objects
    public ReactionTarget Target { get; private set; } = null!;
    public ReactionType Type { get; private set; } = null!;
    public AuthorInfo Author { get; private set; } = null!;

    // Properties
    public DateTime ReactedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedByAliasId { get; set; }

    private Reaction() { }

    /// <summary>
    /// Factory method to create a new reaction
    /// </summary>
    public static Reaction Create(
        string targetType,
        Guid targetId,
        string reactionType,
        Guid authorAliasId,
        Guid authorAliasVersionId)
    {
        var target = new ReactionTarget(targetType, targetId);
        var type = new ReactionType(reactionType);
        var author = new AuthorInfo(authorAliasId, authorAliasVersionId);

        var reaction = new Reaction
        {
            Id = Guid.NewGuid(),
            Target = target,
            Type = type,
            Author = author,
            ReactedAt = DateTime.UtcNow
        };

        reaction.AddDomainEvent(new ReactionCreatedEvent(
            reaction.Id, 
            targetType, 
            targetId, 
            reactionType, 
            authorAliasId));

        return reaction;
    }

    /// <summary>
    /// Change reaction type (e.g., from like to love)
    /// </summary>
    public void ChangeReactionType(string newReactionType, Guid editorAliasId)
    {
        ValidateEditPermission(editorAliasId);
        ValidateNotDeleted();

        var oldType = Type.Value;
        Type = new ReactionType(newReactionType);
        ModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new ReactionTypeChangedEvent(Id, Target.TargetType, Target.TargetId, oldType, newReactionType));
    }

    /// <summary>
    /// Remove reaction (soft delete)
    /// </summary>
    public void Remove(Guid removerAliasId)
    {
        ValidateEditPermission(removerAliasId);
        
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByAliasId = removerAliasId.ToString();

        AddDomainEvent(new ReactionRemovedEvent(Id, Target.TargetType, Target.TargetId, Type.Value, removerAliasId));
    }

    /// <summary>
    /// Restore removed reaction
    /// </summary>
    public void Restore(Guid restorerAliasId)
    {
        ValidateEditPermission(restorerAliasId);
        
        if (!IsDeleted) return;

        IsDeleted = false;
        DeletedAt = null;
        DeletedByAliasId = null;

        AddDomainEvent(new ReactionRestoredEvent(Id, Target.TargetType, Target.TargetId, Type.Value, restorerAliasId));
    }

    // Business logic properties
    public bool IsModified => ModifiedAt.HasValue;
    public bool IsPositiveReaction => Type.IsPositive;
    public bool IsNegativeReaction => Type.IsNegative;
    public bool IsHighWeightReaction => Type.IsHighWeight;
    public bool IsOnPost => Target.IsPost;
    public bool IsOnComment => Target.IsComment;

    // Private validation methods
    private void ValidateEditPermission(Guid editorAliasId)
    {
        if (editorAliasId != Author.AliasId)
            throw new ReactionAuthorMismatchException("Chỉ người tạo phản ứng mới có thể chỉnh sửa.");
    }

    private void ValidateNotDeleted()
    {
        if (IsDeleted)
            throw new DeletedReactionActionException("Không thể thực hiện hành động trên phản ứng đã bị xóa.");
    }
}
