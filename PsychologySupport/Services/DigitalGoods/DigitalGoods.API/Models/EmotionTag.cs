using BuildingBlocks.DDD;
using DigitalGoods.API.Enums;

namespace DigitalGoods.API.Models;

public class EmotionTag : AuditableEntity<Guid>
{
    public string Code { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public Guid? MediaId { get; set; }

    public string? UnicodeCodepoint { get; private set; }

    public string? Topic { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public EmotionTagScope Scope { get; set; }

    public ICollection<DigitalGood> DigitalGoods { get; set; } = new List<DigitalGood>();


    // ✅ Factory method
    public static EmotionTag Create(
        string code,
        string displayName,
        string? unicodeCodepoint,
        string? topic,
        int sortOrder,
        bool isActive,
        EmotionTagScope scope,
        Guid? mediaId,
        string createdBy)
    {
        return new EmotionTag
        {
            Id = Guid.NewGuid(),
            Code = code,
            DisplayName = displayName,
            UnicodeCodepoint = unicodeCodepoint,
            Topic = topic,
            SortOrder = sortOrder,
            IsActive = isActive,
            Scope = scope,
            MediaId = mediaId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }
}
