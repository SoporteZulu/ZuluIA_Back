using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class PuntoFacturacionRepository : BaseRepository<PuntoFacturacion>, IPuntoFacturacionRepository
{
    public PuntoFacturacionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<PuntoFacturacion>> GetActivosBySucursalAsync(
        long sucursalId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.SucursalId == sucursalId && x.Activo)
            .OrderBy(x => x.Numero)
            .ToListAsync(ct);

    public async Task<bool> ExisteNumeroAsync(
        long sucursalId,
        short numero,
        long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(x =>
            x.SucursalId == sucursalId && x.Numero == numero);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<long> GetProximoNumeroComprobanteAsync(
        long puntoFacturacionId,
        long tipoComprobanteId,
        CancellationToken ct = default)
    {
        var punto = await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == puntoFacturacionId, ct);

        if (punto is null)
            throw new InvalidOperationException(
                $"No se encontró el punto de facturación {puntoFacturacionId}.");

        // Buscar el último número emitido para ese punto y tipo
        var ultimoNumero = await Context.Comprobantes
            .AsNoTracking()
            .Where(x =>
                x.PuntoFacturacionId == puntoFacturacionId &&
                x.TipoComprobanteId  == tipoComprobanteId  &&
                x.Estado.ToString()  != "ANULADO")
            .MaxAsync(x => (long?)x.Numero.Numero, ct);

        return (ultimoNumero ?? 0) + 1;
    }
}
