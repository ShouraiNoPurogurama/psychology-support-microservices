namespace Post.Domain.Models;

public partial class IdempotencyKey
{
    public Guid Id { get; set; }

    public Guid Key { get; set; }

    public byte[] RequestFingerprint { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
