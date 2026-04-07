using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace ZuluIA_Back.Application.Common.Extensions;

public static class EfQueryFallbackExtensions
{
    public static IQueryable<T> AsQueryableSafe<T>(this IQueryable<T> source)
        => SupportsAsync(source)
            ? source
            : source is IEnumerable<T> enumerable
                ? MaterializeOrEmpty(enumerable)
                : Enumerable.Empty<T>().AsQueryable();

    public static IQueryable<T> AsNoTrackingSafe<T>(this IQueryable<T> source)
        where T : class
    {
        var query = source.AsQueryableSafe();
        return SupportsAsync(query)
            ? query.AsNoTracking()
            : query;
    }

    public static Task<T?> FirstOrDefaultSafeAsync<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var query = source.AsQueryableSafe();
        return SupportsAsync(query)
            ? EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(query, predicate, cancellationToken)
            : Task.FromResult(query.FirstOrDefault(predicate));
    }

    public static Task<T?> FirstOrDefaultSafeAsync<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default)
    {
        var query = source.AsQueryableSafe();
        return SupportsAsync(query)
            ? EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(query, cancellationToken)
            : Task.FromResult(query.FirstOrDefault());
    }

    public static Task<bool> AnySafeAsync<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var query = source.AsQueryableSafe();
        return SupportsAsync(query)
            ? EntityFrameworkQueryableExtensions.AnyAsync(query, predicate, cancellationToken)
            : Task.FromResult(query.Any(predicate));
    }

    public static Task<List<T>> ToListSafeAsync<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default)
    {
        var query = source.AsQueryableSafe();
        return SupportsAsync(query)
            ? EntityFrameworkQueryableExtensions.ToListAsync(query, cancellationToken)
            : Task.FromResult(query.ToList());
    }

    public static Task<Dictionary<TKey, TSource>> ToDictionarySafeAsync<TSource, TKey>(
        this IQueryable<TSource> source,
        Func<TSource, TKey> keySelector,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        var query = source.AsQueryableSafe();
        return SupportsAsync(query)
            ? EntityFrameworkQueryableExtensions.ToDictionaryAsync(query, keySelector, cancellationToken)
            : Task.FromResult(query.ToDictionary(keySelector));
    }

    public static Task<Dictionary<TKey, TElement>> ToDictionarySafeAsync<TSource, TKey, TElement>(
        this IQueryable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        var query = source.AsQueryableSafe();
        return SupportsAsync(query)
            ? EntityFrameworkQueryableExtensions.ToDictionaryAsync(query, keySelector, elementSelector, cancellationToken)
            : Task.FromResult(query.ToDictionary(keySelector, elementSelector));
    }

    public static Task<TResult?> MaxSafeAsync<TSource, TResult>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, TResult?>> selector,
        CancellationToken cancellationToken = default)
        where TResult : struct
    {
        var query = source.AsQueryableSafe();
        return SupportsAsync(query)
            ? EntityFrameworkQueryableExtensions.MaxAsync(query.Select(selector), cancellationToken)
            : Task.FromResult(query.Select(selector.Compile()).Max());
    }

    private static bool SupportsAsync<T>(IQueryable<T> source)
    {
        try
        {
            return source.Provider is IAsyncQueryProvider;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    private static IQueryable<T> MaterializeOrEmpty<T>(IEnumerable<T> source)
    {
        try
        {
            return source.ToList().AsQueryable();
        }
        catch (NotSupportedException)
        {
            return Enumerable.Empty<T>().AsQueryable();
        }
    }
}
