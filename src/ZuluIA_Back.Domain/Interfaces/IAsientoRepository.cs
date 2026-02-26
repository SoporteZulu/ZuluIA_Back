using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IAsientoRepository : IRepository<Asiento>
{
    Task<long> GetProximoNumeroAsync(long ejercicioId, long sucursalId, CancellationToken ct = default);
    Task<PagedResult<Asiento>> GetPagedAsync(int page, int pageSize, long ejercicioId, long? sucursalId, string? estado, CancellationToken ct = default);
}