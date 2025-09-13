namespace BuildingBlocks.Idempotency;

public interface IIdempotencyHasher
{
    string ComputeHash(object request);
}