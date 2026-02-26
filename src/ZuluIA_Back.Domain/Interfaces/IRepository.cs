using System.Linq.Expressions;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}