namespace BuildingBlocks.Pagination;

public record PaginatedResult<TEntity>(
    int PageIndex,
    int PageSize,
    long TotalCount,
    IEnumerable<TEntity> Data)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
