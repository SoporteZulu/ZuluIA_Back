using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IMovimientoStockRepository : IRepository<MovimientoStock>
{
    Task<PagedResult<MovimientoStock>> GetPagedAsync(
        int page,
        int pageSize,
        long? itemId,
        long? depositoId,
        TipoMovimientoStock? tipo,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default);

    Task<IReadOnlyList<MovimientoStock>> GetByOrigenAsync(
        string origenTabla,
        long origenId,
        CancellationToken ct = default);
}