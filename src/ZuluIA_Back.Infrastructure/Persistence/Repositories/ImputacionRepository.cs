using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class ImputacionRepository(AppDbContext context)
    : BaseRepository<Imputacion>(context), IImputacionRepository
{
    public async Task<IReadOnlyList<Imputacion>> GetByComprobanteOrigenAsync(
        long comprobanteId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.ComprobanteOrigenId == comprobanteId)
            .OrderByDescending(x => x.Fecha)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Imputacion>> GetByComprobanteDestinoAsync(
        long comprobanteId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.ComprobanteDestinoId == comprobanteId)
            .OrderByDescending(x => x.Fecha)
            .ToListAsync(ct);

    public async Task<decimal> GetTotalImputadoAsync(
        long comprobanteDestinoId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.ComprobanteDestinoId == comprobanteDestinoId)
            .SumAsync(x => (decimal?)x.Importe, ct) ?? 0;
}