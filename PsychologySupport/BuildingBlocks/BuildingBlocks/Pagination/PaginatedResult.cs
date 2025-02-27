namespace BuildingBlocks.Pagination;

public record PaginatedResult<TEntity>(
    int PageIndex,
    int PageSize,
    long TotalCount,
    IEnumerable<TEntity> Data)
{
    public PaginatedResult() : this(0, 0, 0, Enumerable.Empty<TEntity>())
    {
        PageIndex = 0;
        PageSize = 0;
        TotalCount = 0;
        Data = new List<TEntity>();
    }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
