namespace Post.Domain.Models;

public partial class GiftsAttach 
{
    public Guid Id { get; set; }

    public string TargetType { get; set; } = null!;

    public Guid TargetId { get; set; }

    public Guid GiftId { get; set; }

    public string? Message { get; set; }

    public Guid SenderAliasId { get; set; }

    public Guid SenderAliasVersionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }
}
