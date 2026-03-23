using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using System.Collections;
using System.Linq.Expressions;

namespace ZuluIA_Back.UnitTests.Helpers;

/// <summary>
/// Helper para crear instancias de DbSet&lt;T&gt; mockeables que soportan
/// las extension methods async de EF Core (FirstOrDefaultAsync, ToListAsync,
/// ToDictionaryAsync, etc.) en pruebas unitarias con NSubstitute.
/// </summary>
public static class MockDbSetHelper
{
    public static DbSet<T> CreateMockDbSet<T>(IEnumerable<T>? data = null)
        where T : class
    {
        var list = data?.ToList() ?? [];
        var queryable = list.AsQueryable();

        var mockDbSet = Substitute.For<DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>>();

        ((IQueryable<T>)mockDbSet).Provider
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        ((IQueryable<T>)mockDbSet).Expression
            .Returns(queryable.Expression);
        ((IQueryable<T>)mockDbSet).ElementType
            .Returns(queryable.ElementType);
        ((IQueryable<T>)mockDbSet).GetEnumerator()
            .Returns(_ => queryable.GetEnumerator());
        ((IAsyncEnumerable<T>)mockDbSet).GetAsyncEnumerator(Arg.Any<CancellationToken>())
            .Returns(_ => new TestAsyncEnumerator<T>(list.GetEnumerator()));

        return mockDbSet;
    }
}

internal sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(inner.MoveNext());
    public ValueTask DisposeAsync() { inner.Dispose(); return ValueTask.CompletedTask; }
}

/// <summary>
/// Strips EF Core-specific IQueryable methods (Include, ThenInclude, AsNoTracking, etc.)
/// that cannot be evaluated by LINQ-to-Objects in unit tests.
/// </summary>
internal sealed class EfQueryableMethodStripper : ExpressionVisitor
{
    private static readonly HashSet<string> _efMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "Include", "ThenInclude", "AsNoTracking", "AsTracking",
        "TagWith", "AsSplitQuery", "AsSingleQuery"
    };

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (_efMethods.Contains(node.Method.Name)
            && node.Method.DeclaringType?.Assembly?.GetName().Name?.StartsWith("Microsoft.EntityFrameworkCore") == true)
        {
            // Return the source argument (first argument) to strip the EF-specific call
            return Visit(node.Arguments[0]);
        }
        return base.VisitMethodCall(node);
    }
}

internal sealed class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
        => new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression)
        => inner.Execute(new EfQueryableMethodStripper().Visit(expression));

    public TResult Execute<TResult>(Expression expression)
        => inner.Execute<TResult>(new EfQueryableMethodStripper().Visit(expression));

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        => new TestAsyncEnumerable<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var resultType = typeof(TResult).GetGenericArguments().Single();
        var executed = Execute(expression);
        return (TResult)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, [executed])!;
    }
}

internal sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
}
