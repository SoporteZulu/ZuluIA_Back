using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class ImputacionRepository(AppDbContext context)
    : BaseRepository<Imputacion>(context), IImputacionRepository
{
    public async Task<IReadOnlyList<Imputacion>> GetByComprobanteOrigenAsync(
        long comprobanteId,
        bool incluirAnuladas = false,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.ComprobanteOrigenId == comprobanteId)
            .Where(x => incluirAnuladas || !x.Anulada)
            .OrderByDescending(x => x.Fecha)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Imputacion>> GetByComprobanteDestinoAsync(
        long comprobanteId,
        bool incluirAnuladas = false,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.ComprobanteDestinoId == comprobanteId)
            .Where(x => incluirAnuladas || !x.Anulada)
            .OrderByDescending(x => x.Fecha)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Imputacion>> GetHistorialByComprobanteAsync(
        long comprobanteId,
        bool incluirAnuladas = true,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.ComprobanteOrigenId == comprobanteId || x.ComprobanteDestinoId == comprobanteId)
            .Where(x => incluirAnuladas || !x.Anulada)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<decimal> GetTotalImputadoAsync(
        long comprobanteDestinoId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.ComprobanteDestinoId == comprobanteDestinoId)
            .Where(x => !x.Anulada)
            .SumAsync(x => (decimal?)x.Importe, ct) ?? 0;
}