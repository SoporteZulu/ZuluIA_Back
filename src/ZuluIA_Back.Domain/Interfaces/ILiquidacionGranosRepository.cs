using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Agro;

namespace ZuluIA_Back.Domain.Interfaces;

public interface ILiquidacionGranosRepository : IRepository<LiquidacionGranos>
{
    Task<PagedResult<LiquidacionGranos>> GetPagedAsync(
        int page,
        int pageSize,
        long? sucursalId,
        long? terceroId,
        string? estado,
        CancellationToken ct = default);

    Task<LiquidacionGranos?> GetByIdConConceptosAsync(long id, CancellationToken ct = default);
}
