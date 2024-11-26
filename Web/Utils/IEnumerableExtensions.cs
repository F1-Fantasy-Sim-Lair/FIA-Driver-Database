using Microsoft.EntityFrameworkCore;

namespace Web.Utils;

public static class IEnumerableExtensions
{
    public static async Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
    {
        if (source is IAsyncEnumerable<TSource> asyncQueryable)
            return await EntityFrameworkQueryableExtensions.ToListAsync(source, cancellationToken);
        // Fallback in testing situations where there is no real async connection
        else
            return [.. source];
    }
}
