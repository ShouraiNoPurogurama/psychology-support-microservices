using Post.Domain.Aggregates.Posts.ValueObjects;
using Post.Domain.Aggregates.Reaction.DomainEvents;
using Post.Domain.Aggregates.Reaction.ValueObjects;
using Post.Domain.Aggregates.Reactions.Enums;
using Post.Domain.Exceptions;

namespace Post.Domain.Aggregates.Reaction;

public sealed class Reaction : AggregateRoot<Guid>, ISoftDeletable
{
    // Value Objects
    public ReactionTarget Target { get; private set; } = null!;
    public ReactionType Type { get; private set; } = null!;
    public AuthorInfo Author { get; private set; } = null!;

    // Properties
    public DateTimeOffset ReactedAt { get; private set; }
    public DateTimeOffset? ModifiedAt { get; private set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedByAliasId { get; set; }

    // EF Core
    private Reaction() { }

    /// <summary>
    /// Tạo tương tác mới.
    /// </summary>
    /// <param name="targetType">"post" hoặc "comment".</param>
    /// <param name="targetId">ID bài viết/bình luận.</param>
    /// <param name="reactionCode">Mã tương tác (like/love/haha...)</param>
    /// <param name="emoji">Emoji/icon (lấy từ DB)</param>
    /// <param name="weight">Trọng số (lấy từ DB)</param>
    /// <param name="isEnabled">Trạng thái kích hoạt loại tương tác (lấy từ DB)</param>
        public static Reaction Create(
            ReactionTargetType targetType,
            Guid targetId,
            string reactionCode,
            string? emoji,
            int weight,
            bool isEnabled,
            Guid reactorAliasId,
            Guid reactorAliasVersionId)
        {
            var reaction = new Reaction
            {
                Id = Guid.NewGuid(),
                Target = ReactionTarget.Create(targetType, targetId),
                Type = ReactionType.Create(reactionCode, emoji, weight, isEnabled),
                Author = AuthorInfo.Create(reactorAliasId, reactorAliasVersionId),
                ReactedAt = DateTimeOffset.UtcNow
            };

            reaction.AddDomainEvent(new ReactionCreatedEvent(
                reaction.Id,
                reaction.Target.TargetType,
                reaction.Target.TargetId,
                reaction.Type.Code,
                reactorAliasId));

            return reaction;
        }

    /// <summary>
    /// Đổi loại tương tác (ví dụ: từ 'like' sang 'love').
    /// </summary>
    public void ChangeReactionType(
        string newReactionCode,
        string? emoji,
        int weight,
        bool isEnabled,
        Guid editorAliasId)
    {
        ValidateEditPermission(editorAliasId);
        ValidateNotDeleted();

        var oldCode = Type.Code;
        Type = ReactionType.Create(newReactionCode, emoji, weight, isEnabled);
        ModifiedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new ReactionTypeChangedEvent(
            Id,
            Target.TargetType,
            Target.TargetId,
            oldCode,
            Type.Code,
            editorAliasId));
    }
    
    public void UpdateType(ReactionType newType, Guid editorAliasId)
    {
        ValidateEditPermission(editorAliasId);
        ValidateNotDeleted();

        if (Type.Code == newType.Code) return;

        var oldCode = Type.Code;
        Type = newType;
        ModifiedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new ReactionTypeChangedEvent(
            Id,
            Target.TargetType,
            Target.TargetId,
            oldCode,
            newType.Code,
            editorAliasId));
    }

    /// <summary>
    /// Xoá mềm tương tác.
    /// </summary>
    public void Delete(Guid removerAliasId)
    {
        ValidateEditPermission(removerAliasId);
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByAliasId = removerAliasId.ToString();

        AddDomainEvent(new ReactionRemovedEvent(
            Id,
            Target.TargetType,
            Target.TargetId,
            Type.Code,
            removerAliasId));
    }

    /// <summary>
    /// Phục hồi tương tác đã xoá mềm.
    /// </summary>
    public void Restore(Guid restorerAliasId)
    {
        ValidateEditPermission(restorerAliasId);
        if (!IsDeleted) return;

        IsDeleted = false;
        DeletedAt = null;
        DeletedByAliasId = null;

        AddDomainEvent(new ReactionRestoredEvent(
            Id,
            Target.TargetType,
            Target.TargetId,
            Type.Code,
            restorerAliasId));
    }

    // Business props
    public bool IsModified => ModifiedAt.HasValue;
    public bool IsPositive => Type.IsPositive;
    public bool IsNegative => Type.IsNegative;
    public bool IsHighWeight => Type.IsHighWeight;
    public bool IsOnPost => Target.IsPost;
    public bool IsOnComment => Target.IsComment;

    // Private validations
    private void ValidateEditPermission(Guid editorAliasId)
    {
        if (editorAliasId != Author.AliasId)
            throw new ReactionAuthorMismatchException("Chỉ người tạo tương tác mới được phép chỉnh sửa.");
    }

    private void ValidateNotDeleted()
    {
        if (IsDeleted)
            throw new DeletedReactionActionException("Không thể thực hiện hành động trên tương tác đã bị xoá.");
    }
}
