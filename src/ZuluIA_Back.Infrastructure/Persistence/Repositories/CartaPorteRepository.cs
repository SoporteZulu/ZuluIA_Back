using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class CartaPorteRepository(AppDbContext context)
    : BaseRepository<CartaPorte>(context), ICartaPorteRepository
{
    public async Task<PagedResult<CartaPorte>> GetPagedAsync(
        int page,
        int pageSize,
        long? comprobanteId,
        EstadoCartaPorte? estado,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (comprobanteId.HasValue)
            query = query.Where(x => x.ComprobanteId == comprobanteId.Value);

        if (estado.HasValue)
            query = query.Where(x => x.Estado == estado.Value);

        if (desde.HasValue)
            query = query.Where(x => x.FechaEmision >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(x => x.FechaEmision <= hasta.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.FechaEmision)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<CartaPorte>(items, page, pageSize, total);
    }

    public async Task<CartaPorte?> GetByNroCtgAsync(
        string nroCtg,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.NroCtg == nroCtg.Trim(), ct);
}