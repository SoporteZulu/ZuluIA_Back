using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IAsientoRepository : IRepository<Asiento>
{
    Task<PagedResult<Asiento>> GetPagedAsync(
        int page,
        int pageSize,
        long ejercicioId,
        long? sucursalId,
        EstadoAsiento? estado,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default);

    Task<Asiento?> GetByIdConLineasAsync(
        long id,
        CancellationToken ct = default);

    Task<long> GetProximoNumeroAsync(
        long ejercicioId,
        long sucursalId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Asiento>> GetByOrigenAsync(
        string origenTabla,
        long origenId,
        CancellationToken ct = default);
}