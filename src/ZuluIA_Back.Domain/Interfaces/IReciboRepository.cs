using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IReciboRepository : IRepository<Recibo>
{
    Task<PagedResult<Recibo>> GetPagedAsync(
        int page,
        int pageSize,
        long? sucursalId,
        long? terceroId,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default);

    Task<Recibo?> GetByIdConItemsAsync(long id, CancellationToken ct = default);

    Task<int> GetUltimoNumeroAsync(long sucursalId, string serie, CancellationToken ct = default);
}
