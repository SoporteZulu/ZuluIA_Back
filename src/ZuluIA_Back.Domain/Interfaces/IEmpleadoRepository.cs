using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IEmpleadoRepository : IRepository<Empleado>
{
    Task<PagedResult<Empleado>> GetPagedAsync(
        int page,
        int pageSize,
        long? sucursalId,
        EstadoEmpleado? estado,
        string? search,
        CancellationToken ct = default);

    Task<Empleado?> GetByLegajoAsync(
        string legajo,
        CancellationToken ct = default);

    Task<bool> ExisteLegajoAsync(
        string legajo,
        long? excludeId,
        CancellationToken ct = default);
}