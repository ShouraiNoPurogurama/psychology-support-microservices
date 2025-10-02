using Post.Domain.Aggregates.CategoryTags.DomainEvents;
using Post.Domain.Exceptions;

namespace Post.Domain.Aggregates.CategoryTags;

/// <summary>
/// CategoryTag aggregate root - represents master data for post categorization
/// Used for organizing posts by topics, emotions, or content types
/// </summary>
public sealed class CategoryTag : AggregateRoot<Guid>
{
    public string Code { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string? Color { get; private set; }
    public string? UnicodeCodepoint { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }

    private CategoryTag() { }

    /// <summary>
    /// Factory method to create a new category tag
    /// </summary>
    public static CategoryTag Create(
        string code,
        string displayName,
        string? color = null,
        string? unicodeCodepoint = null,
        int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidPostDataException("Mã danh mục không được để trống.");

        if (string.IsNullOrWhiteSpace(displayName))
            throw new InvalidPostDataException("Tên hiển thị danh mục không được để trống.");

        if (code.Length > 50)
            throw new InvalidPostDataException("Mã danh mục không được vượt quá 50 ký tự.");

        if (displayName.Length > 200)
            throw new InvalidPostDataException("Tên hiển thị không được vượt quá 200 ký tự.");

        var categoryTag = new CategoryTag
        {
            Id = Guid.NewGuid(),
            Code = code.ToUpperInvariant().Trim(),
            DisplayName = displayName.Trim(),
            Color = color?.Trim(),
            UnicodeCodepoint = unicodeCodepoint?.Trim(),
            IsActive = true,
            SortOrder = Math.Max(0, sortOrder),
            CreatedAt = DateTimeOffset.UtcNow
        };

        categoryTag.AddDomainEvent(new CategoryTagCreatedEvent(categoryTag.Id, categoryTag.Code, categoryTag.DisplayName));
        return categoryTag;
    }

    /// <summary>
    /// Update display name and visual properties
    /// </summary>
    public void UpdateDisplayInfo(string displayName, string? color = null, string? unicodeCodepoint = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new InvalidPostDataException("Tên hiển thị danh mục không được để trống.");

        if (displayName.Length > 200)
            throw new InvalidPostDataException("Tên hiển thị không được vượt quá 200 ký tự.");

        var oldDisplayName = DisplayName;
        DisplayName = displayName.Trim();
        Color = color?.Trim();
        UnicodeCodepoint = unicodeCodepoint?.Trim();

        AddDomainEvent(new CategoryTagUpdatedEvent(Id, Code, oldDisplayName, DisplayName));
    }

    /// <summary>
    /// Activate or deactivate the category tag
    /// </summary>
    public void SetActiveStatus(bool isActive)
    {
        if (IsActive == isActive) return;

        IsActive = isActive;
        AddDomainEvent(new CategoryTagStatusChangedEvent(Id, Code, isActive));
    }

    /// <summary>
    /// Update sort order for category display ordering
    /// </summary>
    public void UpdateSortOrder(int newSortOrder)
    {
        SortOrder = Math.Max(0, newSortOrder);
    }

    // Business logic properties
    public bool HasColor => !string.IsNullOrWhiteSpace(Color);
    public bool HasEmoji => !string.IsNullOrWhiteSpace(UnicodeCodepoint);
    public bool IsSystemCategory => Code.StartsWith("SYS_");
    public bool IsUserCategory => !IsSystemCategory;
}
