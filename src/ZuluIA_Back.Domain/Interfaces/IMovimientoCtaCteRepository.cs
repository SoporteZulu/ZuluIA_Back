using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IMovimientoCtaCteRepository : IRepository<MovimientoCtaCte>
{
    Task<PagedResult<MovimientoCtaCte>> GetPagedAsync(
        int page,
        int pageSize,
        long terceroId,
        long? sucursalId,
        long? monedaId,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default);
}