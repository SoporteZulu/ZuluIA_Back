using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class AsientoRepository(AppDbContext context)
    : BaseRepository<Asiento>(context), IAsientoRepository
{
    public async Task<PagedResult<Asiento>> GetPagedAsync(
        int page, int pageSize,
        long ejercicioId,
        long? sucursalId,
        EstadoAsiento? estado,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Where(x => x.EjercicioId == ejercicioId);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (estado.HasValue)
            query = query.Where(x => x.Estado == estado.Value);

        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Numero)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Asiento>(items, page, pageSize, total);
    }

    public async Task<Asiento?> GetByIdConLineasAsync(
        long id,
        CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Lineas)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<long> GetProximoNumeroAsync(
        long ejercicioId,
        long sucursalId,
        CancellationToken ct = default)
    {
        var ultimo = await DbSet
            .AsNoTracking()
            .Where(x =>
                x.EjercicioId == ejercicioId &&
                x.SucursalId  == sucursalId  &&
                x.Estado      != EstadoAsiento.Anulado)
            .MaxAsync(x => (long?)x.Numero, ct);

        return (ultimo ?? 0) + 1;
    }

    public async Task<IReadOnlyList<Asiento>> GetByOrigenAsync(
        string origenTabla,
        long origenId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x =>
                x.OrigenTabla == origenTabla &&
                x.OrigenId    == origenId)
            .OrderByDescending(x => x.Fecha)
            .ToListAsync(ct);
}
