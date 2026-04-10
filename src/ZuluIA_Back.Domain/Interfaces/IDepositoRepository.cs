using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IDepositoRepository : IRepository<Deposito>
{
    Task<IReadOnlyList<Deposito>> GetBySucursalAsync(
        long sucursalId,
        bool incluirInactivos = false,
        CancellationToken ct = default);

    Task<IReadOnlyList<Deposito>> GetActivosBySucursalAsync(
        long sucursalId,
        CancellationToken ct = default);

    Task<Deposito?> GetDefaultBySucursalAsync(
        long sucursalId,
        CancellationToken ct = default);
}