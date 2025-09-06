namespace Post.Domain.Models;

public sealed class ReactionPack : Entity<Guid>
{
    public string Code { get; set; } = null!;        //unique: "cute_pack_2025"
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    // Điều hướng (optional)
    public ICollection<ReactionType> ReactionTypes { get; set; } = new List<ReactionType>();
    public ICollection<ReactionPackItem> ReactionPackItems { get; set; } = new List<ReactionPackItem>();
}