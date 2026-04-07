using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class ChequeRepository(AppDbContext context)
    : BaseRepository<Cheque>(context), IChequeRepository
{
    public async Task<PagedResult<Cheque>> GetPagedAsync(
        int page,
        int pageSize,
        long? cajaId,
        long? terceroId,
        EstadoCheque? estado,
        ZuluIA_Back.Domain.Enums.TipoCheque? tipo,
        bool? esALaOrden,
        bool? esCruzado,
        string? banco,
        string? nroCheque,
        string? titular,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (cajaId.HasValue)
            query = query.Where(x => x.CajaId == cajaId.Value);

        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);

        if (estado.HasValue)
            query = query.Where(x => x.Estado == estado.Value);

        if (tipo.HasValue)
            query = query.Where(x => x.Tipo == tipo.Value);

        if (esALaOrden.HasValue)
            query = query.Where(x => x.EsALaOrden == esALaOrden.Value);

        if (esCruzado.HasValue)
            query = query.Where(x => x.EsCruzado == esCruzado.Value);

        if (!string.IsNullOrWhiteSpace(banco))
            query = query.Where(x => x.Banco.Contains(banco.Trim()));

        if (!string.IsNullOrWhiteSpace(nroCheque))
            query = query.Where(x => x.NroCheque.Contains(nroCheque.Trim()));

        if (!string.IsNullOrWhiteSpace(titular))
            query = query.Where(x => x.Titular != null && x.Titular.Contains(titular.Trim()));

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

        return new PagedResult<Cheque>(items, page, pageSize, total);
    }

    public async Task<IReadOnlyList<Cheque>> GetCarteraAsync(
        long cajaId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.CajaId == cajaId && x.Estado == EstadoCheque.Cartera)
            .OrderBy(x => x.FechaVencimiento)
            .ToListAsync(ct);
}