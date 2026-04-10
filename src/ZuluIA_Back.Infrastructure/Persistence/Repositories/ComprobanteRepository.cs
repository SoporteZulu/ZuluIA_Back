using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class ComprobanteRepository(AppDbContext context)
    : BaseRepository<Comprobante>(context), IComprobanteRepository
{
    public async Task<PagedResult<Comprobante>> GetPagedAsync(
        int page,
        int pageSize,
        long? sucursalId,
        long? terceroId,
        long? tipoComprobanteId,
        bool? esVenta,
        bool? esCompra,
        EstadoComprobante? estado,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default)
    {
        IQueryable<Comprobante> query = DbSet
            .AsNoTracking()
            .Include(x => x.Cot);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);

        if (tipoComprobanteId.HasValue)
            query = query.Where(x => x.TipoComprobanteId == tipoComprobanteId.Value);

        if (esVenta.HasValue || esCompra.HasValue)
        {
            var tipoIds = Context.TiposComprobante
                .AsNoTracking()
                .Where(x => !esVenta.HasValue || x.EsVenta == esVenta.Value)
                .Where(x => !esCompra.HasValue || x.EsCompra == esCompra.Value)
                .Select(x => x.Id);

            query = query.Where(x => tipoIds.Contains(x.TipoComprobanteId));
        }

        if (estado.HasValue)
            query = query.Where(x => x.Estado == estado.Value);

        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Comprobante>(items, page, pageSize, total);
    }

    public async Task<Comprobante?> GetByIdConItemsAsync(
        long id,
        CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Items)
            .Include(x => x.Cot)
            .Include(x => x.Atributos)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Comprobante?> GetByNumeroAsync(
        long sucursalId,
        long tipoComprobanteId,
        short prefijo,
        long numero,
        CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Items)
            .Include(x => x.Cot)
            .Include(x => x.Atributos)
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.SucursalId         == sucursalId         &&
                x.TipoComprobanteId  == tipoComprobanteId  &&
                x.Numero.Prefijo     == prefijo            &&
                x.Numero.Numero      == numero,
                ct);

    public async Task<IReadOnlyList<Comprobante>> GetSaldoPendienteByTerceroAsync(
        long terceroId,
        long? sucursalId,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(x =>
                x.TerceroId == terceroId &&
                x.Saldo > 0 &&
                x.Estado != EstadoComprobante.Anulado);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        return await query
            .OrderBy(x => x.Fecha)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Comprobante>> GetDevolucionesPendientesAutorizacionAsync(
        long? sucursalId,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x =>
                x.MotivoDevolucion.HasValue &&
                !x.AutorizadorDevolucionId.HasValue &&
                x.Estado != EstadoComprobante.Anulado &&
                !x.IsDeleted);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);

        return await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Comprobante>> GetDevolucionesByMotivoAsync(
        MotivoDevolucion motivo,
        long? terceroId,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x =>
                x.MotivoDevolucion == motivo &&
                x.Estado != EstadoComprobante.Anulado &&
                !x.IsDeleted);

        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);

        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);

        return await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Comprobante>> GetPedidosPendientesAsync(
        long? terceroId,
        long? sucursalId,
        bool? soloAtrasados,
        DateOnly? fechaEntregaDesde,
        DateOnly? fechaEntregaHasta,
        CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var query = DbSet
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x =>
                x.EstadoPedido.HasValue &&
                x.Estado != EstadoComprobante.Anulado &&
                !x.IsDeleted &&
                x.EstadoPedido != EstadoPedido.Cerrado &&
                x.EstadoPedido != EstadoPedido.Anulado &&
                x.EstadoPedido != EstadoPedido.Completado);

        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (fechaEntregaDesde.HasValue)
            query = query.Where(x => x.FechaEntregaCompromiso.HasValue && x.FechaEntregaCompromiso.Value >= fechaEntregaDesde.Value);

        if (fechaEntregaHasta.HasValue)
            query = query.Where(x => x.FechaEntregaCompromiso.HasValue && x.FechaEntregaCompromiso.Value <= fechaEntregaHasta.Value);

        if (soloAtrasados == true)
            query = query.Where(x => x.FechaEntregaCompromiso.HasValue && x.FechaEntregaCompromiso.Value < hoy);
        else if (soloAtrasados == false)
            query = query.Where(x => !x.FechaEntregaCompromiso.HasValue || x.FechaEntregaCompromiso.Value >= hoy);

        return await query
            .OrderBy(x => x.FechaEntregaCompromiso ?? x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);
    }

    public async Task<long> GetProximoNumeroAsync(
        long puntoFacturacionId,
        long tipoComprobanteId,
        CancellationToken ct = default)
    {
        var ultimo = await DbSet
            .AsNoTracking()
            .Where(x =>
                x.PuntoFacturacionId == puntoFacturacionId &&
                x.TipoComprobanteId  == tipoComprobanteId  &&
                x.Estado             != EstadoComprobante.Anulado)
            .MaxAsync(x => (long?)x.Numero.Numero, ct);

        return (ultimo ?? 0) + 1;
    }

    public override async Task<Comprobante?> GetByIdAsync(long id, CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Items)
            .Include(x => x.Cot)
            .Include(x => x.Atributos)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
}
