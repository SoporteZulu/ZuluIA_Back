using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IComprobanteRepository : IRepository<Comprobante>
{
    Task<PagedResult<Comprobante>> GetPagedAsync(
        int page,
        int pageSize,
        long? sucursalId,
        long? terceroId,
        long? tipoComprobanteId,
        EstadoComprobante? estado,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default);

    Task<Comprobante?> GetByNumeroAsync(
        long sucursalId,
        long tipoComprobanteId,
        short prefijo,
        long numero,
        CancellationToken ct = default);

    Task<long> GetProximoNumeroAsync(
        long puntoFacturacionId,
        long tipoComprobanteId,
        CancellationToken ct = default);

    Task<Comprobante?> GetByIdConItemsAsync(
        long id,
        CancellationToken ct = default);

    Task<IReadOnlyList<Comprobante>> GetSaldoPendienteByTerceroAsync(
        long terceroId,
        long? sucursalId,
        CancellationToken ct = default);
}
