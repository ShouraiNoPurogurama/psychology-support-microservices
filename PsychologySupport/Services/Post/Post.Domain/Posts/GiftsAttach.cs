namespace Post.Domain.Posts;

public partial class GiftsAttach : SoftDeletableEntity<Guid>
{

    public string TargetType { get; set; } = null!;

    public Guid TargetId { get; set; }

    public Guid GiftId { get; set; }

    public string? Message { get; set; }

    public Guid SenderAliasId { get; set; }

    public Guid SenderAliasVersionId { get; set; }
}
