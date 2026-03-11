using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IOrdenTrabajoRepository : IRepository<OrdenTrabajo>
{
    Task<PagedResult<OrdenTrabajo>> GetPagedAsync(
        int page,
        int pageSize,
        long? sucursalId,
        long? formulaId,
        EstadoOrdenTrabajo? estado,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default);
}