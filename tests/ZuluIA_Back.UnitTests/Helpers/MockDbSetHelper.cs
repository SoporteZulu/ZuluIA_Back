using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
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
        return new TestAsyncDbSet<T>(list);
    }
}

internal sealed class TestAsyncDbSet<T>(IEnumerable<T> data) : DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>
    where T : class
{
    private readonly List<T> _data = data.ToList();

    private IQueryable<T> Queryable => _data.AsQueryable();

    public Type ElementType => Queryable.ElementType;
    public Expression Expression => Queryable.Expression;
    public IQueryProvider Provider => new TestAsyncQueryProvider<T>(Queryable.Provider);
    Type IQueryable.ElementType => ElementType;
    Expression IQueryable.Expression => Expression;
    IQueryProvider IQueryable.Provider => Provider;

    public override IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(Queryable.GetEnumerator());

    public override IEntityType EntityType
        => throw new NotSupportedException("EntityType metadata is not supported in test DbSets.");

    public override LocalView<T> Local
        => throw new NotSupportedException("Local view is not supported in test DbSets.");

    public override EntityEntry<T> Add(T entity)
    {
        _data.Add(entity);
        return null!;
    }

    public override ValueTask<EntityEntry<T>> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        _data.Add(entity);
        return ValueTask.FromResult<EntityEntry<T>>(null!);
    }

    public override EntityEntry<T> Remove(T entity)
    {
        _data.Remove(entity);
        return null!;
    }

    public override ValueTask<T?> FindAsync(params object?[]? keyValues)
        => ValueTask.FromResult(FindEntity(keyValues));

    public override ValueTask<T?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
        => ValueTask.FromResult(FindEntity(keyValues));

    private T? FindEntity(object?[]? keyValues)
    {
        if (keyValues is null || keyValues.Length != 1 || keyValues[0] is null)
            return null;

        var keyProperty = typeof(T).GetProperty("Id")
            ?? typeof(T).GetProperties().FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.Ordinal));

        if (keyProperty is null)
            throw new NotSupportedException($"FindAsync is not supported for {typeof(T).Name} without an Id property.");

        var keyValue = keyValues[0];
        return _data.FirstOrDefault(entity => Equals(keyProperty.GetValue(entity), keyValue));
    }

    public IEnumerator<T> GetEnumerator() => Queryable.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Queryable.GetEnumerator();
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
