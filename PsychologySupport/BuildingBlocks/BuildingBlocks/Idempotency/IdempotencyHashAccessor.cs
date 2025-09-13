namespace BuildingBlocks.Idempotency;

public sealed class IdempotencyHashAccessor : IIdempotencyHashAccessor
{
    private static readonly AsyncLocal<string?> _ambient = new();
    public string? RequestHash
    {
        get => _ambient.Value;
        set => _ambient.Value = value;
    }
}