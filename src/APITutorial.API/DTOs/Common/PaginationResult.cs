using Microsoft.EntityFrameworkCore;

namespace APITutorial.API.DTOs.Common;

public sealed record PaginationResult<T> : ICollectionResponse<T>
{
    public required List<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPage => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPage;

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "Factory method pattern is appropriate here.")]
    public static async Task<PaginationResult<T>> CreateAsync(IQueryable<T> query, int page, int pageSize)
    {
        int totalCount = await query.CountAsync();
        
        List<T> item = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return new PaginationResult<T>
        {
            Items = item,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
