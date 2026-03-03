using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class CajaRepository(AppDbContext context)
    : BaseRepository<CajaCuentaBancaria>(context), ICajaRepository
{
    public async Task<IReadOnlyList<CajaCuentaBancaria>> GetActivasBySucursalAsync(
        long sucursalId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.SucursalId == sucursalId && x.Activa)
            .OrderBy(x => x.Descripcion)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CajaCuentaBancaria>> GetCajasByUsuarioAsync(
        long usuarioId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId && x.Activa && x.EsCaja)
            .OrderBy(x => x.Descripcion)
            .ToListAsync(ct);

    public async Task<CajaCuentaBancaria?> GetCajaUsuarioActivaAsync(
        long usuarioId,
        long sucursalId,
        CancellationToken ct = default) =>
        await DbSet
            .FirstOrDefaultAsync(x =>
                x.UsuarioId  == usuarioId  &&
                x.SucursalId == sucursalId &&
                x.EsCaja     == true       &&
                x.Activa     == true,
                ct);
}