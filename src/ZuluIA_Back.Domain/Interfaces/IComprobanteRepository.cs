using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IComprobanteRepository : IRepository<Comprobante>
{
    Task<Comprobante?> GetByNumeroAsync(long sucursalId, long tipoComprobanteId, short prefijo, long numero, CancellationToken ct = default);
    Task<long> GetProximoNumeroAsync(long sucursalId, long tipoComprobanteId, short prefijo, CancellationToken ct = default);
    Task<PagedResult<Comprobante>> GetPagedAsync(int page, int pageSize, long? sucursalId, long? terceroId, long? tipoId, string? estado, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
}