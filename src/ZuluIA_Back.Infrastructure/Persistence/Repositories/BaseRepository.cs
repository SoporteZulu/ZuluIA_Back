using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class BaseRepository<T>(AppDbContext context) : IRepository<T>
    where T : BaseEntity
{
    protected readonly AppDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(long id, CancellationToken ct = default) =>
        await DbSet.FindAsync([id], ct);

    public async Task<IList<T>> GetAllAsync(CancellationToken ct = default) =>
        await DbSet.ToListAsync(ct);

    public async Task<IList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default) =>
        await DbSet.Where(predicate).ToListAsync(ct);

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(predicate, ct);

    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default) =>
        await DbSet.AnyAsync(predicate, ct);

    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default) =>
        predicate is null
            ? await DbSet.CountAsync(ct)
            : await DbSet.CountAsync(predicate, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await DbSet.AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default) =>
        await DbSet.AddRangeAsync(entities, ct);

    public void Update(T entity) =>
        DbSet.Update(entity);

    public void Remove(T entity) =>
        DbSet.Remove(entity);
}