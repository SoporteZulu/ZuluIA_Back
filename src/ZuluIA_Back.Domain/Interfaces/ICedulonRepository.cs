using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Interfaces;

public interface ICedulonRepository : IRepository<Cedulon>
{
    Task<PagedResult<Cedulon>> GetPagedAsync(
        int page,
        int pageSize,
        long? terceroId,
        long? sucursalId,
        EstadoCedulon? estado,
        CancellationToken ct = default);

    Task<Cedulon?> GetByNroAsync(
        string nroCedulon,
        CancellationToken ct = default);
}