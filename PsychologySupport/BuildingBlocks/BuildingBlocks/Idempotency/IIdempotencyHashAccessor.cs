namespace BuildingBlocks.Idempotency;

public interface IIdempotencyHashAccessor
{
    string? RequestHash { get; set; }
}