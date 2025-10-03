namespace BuildingBlocks.Pagination;

/// <summary>
/// Represents a paginated result set.
/// </summary>
/// <typeparam name="TEntity">The type of the entities in the result.</typeparam>
public record PaginatedResult<TEntity>(
    int PageIndex,      
    int PageSize,       
    long TotalCount,  
    IEnumerable<TEntity> Data)
{
    /// <summary>
    /// Parameterless constructor for deserialization.
    /// </summary>
    public PaginatedResult() : this(1, 10, 0, Enumerable.Empty<TEntity>())
    {
    }
    
    /// <summary>
    /// Calculates the total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Indicates if there is a page before the current one.
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// Indicates if there is a page after the current one.
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;

    // --- UX/UI Helper Properties ---

    /// <summary>
    /// Indicates if the current page is the first page.
    /// </summary>
    public bool IsFirstPage => PageIndex == 1;

    /// <summary>
    /// Indicates if the current page is the last page.
    /// </summary>
    public bool IsLastPage => PageIndex == TotalPages;

    /// <summary>
    /// The 1-based index of the first item on the current page.
    /// Returns 0 if there are no items.
    /// </summary>
    public long FirstItemIndex => TotalCount > 0 ? ((long)(PageIndex - 1) * PageSize) + 1 : 0;

    /// <summary>
    /// The 1-based index of the last item on the current page.
    /// </summary>
    public long LastItemIndex => TotalCount > 0 ? Math.Min((long)PageIndex * PageSize, TotalCount) : 0;
    
    public static PaginatedResult<TEntity> Empty(int pageIndex, int pageSize) => new(pageIndex, pageSize, 0, Enumerable.Empty<TEntity>());
}