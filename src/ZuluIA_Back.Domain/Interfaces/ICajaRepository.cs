using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Domain.Interfaces;

public interface ICajaRepository : IRepository<CajaCuentaBancaria>
{
    Task<IReadOnlyList<CajaCuentaBancaria>> GetActivasBySucursalAsync(
        long sucursalId,
        CancellationToken ct = default);

    Task<IReadOnlyList<CajaCuentaBancaria>> GetCajasByUsuarioAsync(
        long usuarioId,
        CancellationToken ct = default);

    Task<CajaCuentaBancaria?> GetCajaUsuarioActivaAsync(
        long usuarioId,
        long sucursalId,
        CancellationToken ct = default);
}