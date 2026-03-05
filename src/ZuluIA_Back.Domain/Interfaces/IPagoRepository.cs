using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IPagoRepository : IRepository<Pago>
{
    Task<PagedResult<Pago>> GetPagedAsync(
        int page,
        int pageSize,
        long? sucursalId,
        long? terceroId,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default);

    Task<Pago?> GetByIdConMediosAsync(
        long id,
        CancellationToken ct = default);
}