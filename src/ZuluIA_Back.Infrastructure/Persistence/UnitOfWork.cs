using Microsoft.EntityFrameworkCore.Storage;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default) =>
        _transaction = await context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No hay una transacción activa.");

        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No hay una transacción activa.");

        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _transaction?.Dispose();
            context.Dispose();
        }

        _disposed = true;
    }
}