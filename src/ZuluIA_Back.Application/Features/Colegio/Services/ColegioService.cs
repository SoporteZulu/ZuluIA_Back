using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Colegio.Commands;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Application.Features.Ventas.Services;
using ZuluIA_Back.Domain.Entities.Colegio;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Application.Features.Tesoreria.Services;

namespace ZuluIA_Back.Application.Features.Colegio.Services;

public class ColegioService(
    IApplicationDbContext db,
    IRepository<PlanGeneralColegio> planRepo,
    IRepository<LoteColegio> loteRepo,
    IRepository<CobinproColegioOperacion> cobinproRepo,
    IRepository<ColegioReciboDetalle> reciboDetalleRepo,
    IRepository<Cobro> cobroRepo,
    ICedulonRepository cedulonRepo,
    IComprobanteRepository comprobanteRepo,
    TesoreriaService tesoreriaService,
    CircuitoVentasService circuitoVentas,
    ICurrentUserService currentUser)
{
    public async Task<PlanGeneralColegio> CrearPlanGeneralAsync(CreatePlanGeneralColegioCommand request, CancellationToken ct)
    {
        if (await db.ColegioPlanesGenerales.AsNoTracking().AnyAsync(x => x.SucursalId == request.SucursalId && x.Codigo == request.Codigo.Trim().ToUpperInvariant(), ct))
            throw new InvalidOperationException($"Ya existe un plan general de colegio con código '{request.Codigo}'.");

        await ValidarReferenciasPlanGeneralAsync(request.PlanPagoId, request.TipoComprobanteId, request.ItemId, request.MonedaId, ct);

        var plan = PlanGeneralColegio.Crear(request.SucursalId, request.PlanPagoId, request.TipoComprobanteId, request.ItemId, request.MonedaId, request.Codigo, request.Descripcion, request.ImporteBase, request.Observacion, currentUser.UserId);
        await planRepo.AddAsync(plan, ct);
        return plan;
    }

    public async Task ActualizarPlanGeneralAsync(UpdatePlanGeneralColegioCommand request, CancellationToken ct)
    {
        var plan = await db.ColegioPlanesGenerales.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró el plan general de colegio ID {request.Id}.");

        if (await db.ColegioPlanesGenerales.AsNoTracking().AnyAsync(x => x.Id != request.Id && x.SucursalId == plan.SucursalId && x.Codigo == request.Codigo.Trim().ToUpperInvariant() && !x.IsDeleted, ct))
            throw new InvalidOperationException($"Ya existe un plan general de colegio con código '{request.Codigo}'.");

        await ValidarReferenciasPlanGeneralAsync(request.PlanPagoId, request.TipoComprobanteId, request.ItemId, request.MonedaId, ct);

        plan.Actualizar(request.PlanPagoId, request.TipoComprobanteId, request.ItemId, request.MonedaId, request.Codigo, request.Descripcion, request.ImporteBase, request.Observacion, currentUser.UserId);
        planRepo.Update(plan);
    }

    public async Task DesactivarPlanGeneralAsync(long id, CancellationToken ct)
    {
        var plan = await db.ColegioPlanesGenerales.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró el plan general de colegio ID {id}.");

        if (await db.ColegioLotes.AsNoTracking().AnyAsync(x => x.PlanGeneralColegioId == id && x.Estado != EstadoLoteColegio.Cerrado && !x.IsDeleted, ct))
            throw new InvalidOperationException("No se puede desactivar un plan general con lotes pendientes de cierre.");

        plan.Desactivar(currentUser.UserId);
        planRepo.Update(plan);
    }

    public async Task<LoteColegio> CrearLoteAsync(CreateLoteColegioCommand request, CancellationToken ct)
    {
        if (!await db.ColegioPlanesGenerales.AsNoTracking().AnyAsync(x => x.Id == request.PlanGeneralColegioId && x.Activo, ct))
            throw new InvalidOperationException($"No se encontró el plan general de colegio ID {request.PlanGeneralColegioId}.");

        if (await db.ColegioLotes.AsNoTracking().AnyAsync(x => x.Codigo == request.Codigo.Trim().ToUpperInvariant(), ct))
            throw new InvalidOperationException($"Ya existe un lote de colegio con código '{request.Codigo}'.");

        var lote = LoteColegio.Crear(request.PlanGeneralColegioId, request.Codigo, request.FechaEmision, request.FechaVencimiento, request.Observacion, currentUser.UserId);
        await loteRepo.AddAsync(lote, ct);
        return lote;
    }

    public async Task ActualizarLoteAsync(UpdateLoteColegioCommand request, CancellationToken ct)
    {
        var lote = await db.ColegioLotes.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró el lote de colegio ID {request.Id}.");

        lote.Actualizar(request.FechaEmision, request.FechaVencimiento, request.Observacion, currentUser.UserId);
        loteRepo.Update(lote);
    }

    public async Task<IReadOnlyList<long>> EmitirCedulonesAsync(long loteId, IReadOnlyList<long> terceroIds, CancellationToken ct)
    {
        var lote = await db.ColegioLotes.FirstOrDefaultAsync(x => x.Id == loteId, ct)
            ?? throw new InvalidOperationException($"No se encontró el lote de colegio ID {loteId}.");

        var plan = await db.ColegioPlanesGenerales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == lote.PlanGeneralColegioId && x.Activo, ct)
            ?? throw new InvalidOperationException($"No se encontró el plan general de colegio ID {lote.PlanGeneralColegioId}.");

        var planPago = await db.PlanesPago.AsNoTracking().FirstOrDefaultAsync(x => x.Id == plan.PlanPagoId && x.Activo, ct)
            ?? throw new InvalidOperationException($"No se encontró el plan de pago ID {plan.PlanPagoId}.");

        var ids = new List<long>();
        var emitidos = 0;
        var existentes = await db.Cedulones.AsNoTracking().CountAsync(x => x.LoteColegioId == loteId, ct);

        foreach (var terceroId in terceroIds.Distinct())
        {
            if (await db.Cedulones.AsNoTracking().AnyAsync(x => x.LoteColegioId == loteId && x.TerceroId == terceroId, ct))
                throw new InvalidOperationException($"El tercero ID {terceroId} ya posee un cedulón en el lote {lote.Codigo}.");

            var importe = planPago.CalcularTotalConInteres(plan.ImporteBase);
            var nroCedulon = $"{lote.Codigo}-{(++existentes):D4}";
            var cedulon = Cedulon.Crear(terceroId, plan.SucursalId, plan.PlanPagoId, nroCedulon, lote.FechaEmision, lote.FechaVencimiento, importe, currentUser.UserId);
            cedulon.VincularColegio(plan.Id, lote.Id, currentUser.UserId);
            await cedulonRepo.AddAsync(cedulon, ct);
            ids.Add(cedulon.Id);
            emitidos++;
        }

        lote.MarcarEmitido(emitidos, currentUser.UserId);
        loteRepo.Update(lote);
        return ids;
    }

    public async Task<long> CancelarDeudaAsync(CancelarDeudaColegioCommand request, CancellationToken ct)
    {
        return await RegistrarCobroColegioAsync(
            request.SucursalId,
            request.TerceroId,
            request.Fecha,
            request.MonedaId,
            request.Cotizacion,
            request.CajaId,
            request.FormaPagoId,
            request.Observacion,
            request.Cedulones,
            ct);
    }

    public async Task<IReadOnlyList<long>> CancelarDeudaMasivaAsync(CancelarDeudaColegioMasivaCommand request, CancellationToken ct)
    {
        var cedulones = await db.Cedulones
            .Where(x => request.Cedulones.Select(c => c.CedulonId).Contains(x.Id))
            .ToListAsync(ct);

        var ids = new List<long>();
        foreach (var grupo in request.Cedulones.GroupBy(x => cedulones.First(c => c.Id == x.CedulonId).TerceroId))
        {
            var cobroId = await RegistrarCobroColegioAsync(
                request.SucursalId,
                grupo.Key,
                request.Fecha,
                request.MonedaId,
                request.Cotizacion,
                request.CajaId,
                request.FormaPagoId,
                request.Observacion,
                grupo.Select(x => new CancelacionCedulonColegioInput(x.CedulonId, x.Importe)).ToList(),
                ct);
            ids.Add(cobroId);
        }

        return ids;
    }

    public async Task<CobinproColegioOperacion> RegistrarCobinproAsync(RegistrarCobinproColegioCommand request, CancellationToken ct)
    {
        if (await db.ColegioCobinproOperaciones.AsNoTracking().AnyAsync(x => x.ReferenciaExterna == request.ReferenciaExterna.Trim().ToUpperInvariant(), ct))
            throw new InvalidOperationException($"Ya existe una operación COBINPRO con referencia '{request.ReferenciaExterna}'.");

        var cedulon = await cedulonRepo.GetByIdAsync(request.CedulonId, ct)
            ?? throw new InvalidOperationException($"No se encontró el cedulón ID {request.CedulonId}.");

        var cobroId = await RegistrarCobroColegioAsync(
            cedulon.SucursalId,
            cedulon.TerceroId,
            request.Fecha,
            request.MonedaId,
            request.Cotizacion,
            request.CajaId,
            request.FormaPagoId,
            request.Observacion,
            [new CancelacionCedulonColegioInput(cedulon.Id, request.Importe)],
            ct);

        var operacion = CobinproColegioOperacion.Registrar(cedulon.Id, cedulon.TerceroId, cedulon.SucursalId, cobroId, request.Fecha, request.Importe, request.ReferenciaExterna, request.Observacion, currentUser.UserId);
        await cobinproRepo.AddAsync(operacion, ct);
        return operacion;
    }

    public async Task ConciliarCobinproAsync(long id, bool confirmar, string? observacion, CancellationToken ct)
    {
        var operacion = await db.ColegioCobinproOperaciones.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró la operación COBINPRO ID {id}.");

        if (confirmar)
            operacion.Confirmar(observacion, currentUser.UserId);
        else
            operacion.Rechazar(observacion, currentUser.UserId);

        cobinproRepo.Update(operacion);
    }

    public async Task<IReadOnlyList<long>> FacturarCedulonesAsync(FacturarCedulonesColegioCommand request, CancellationToken ct)
    {
        var cedulones = await db.Cedulones
            .Where(x => request.CedulonIds.Contains(x.Id))
            .OrderBy(x => x.Id)
            .ToListAsync(ct);

        var ids = new List<long>();
        foreach (var cedulon in cedulones)
        {
            if (cedulon.ComprobanteId.HasValue)
                throw new InvalidOperationException($"El cedulón {cedulon.NroCedulon} ya posee un comprobante asociado.");
            if (!cedulon.PlanGeneralColegioId.HasValue)
                throw new InvalidOperationException($"El cedulón {cedulon.NroCedulon} no está vinculado a un plan general de colegio.");
            if (cedulon.ImportePagado > 0)
                throw new InvalidOperationException($"El cedulón {cedulon.NroCedulon} ya posee pagos registrados y no admite facturación automática.");

            var plan = await db.ColegioPlanesGenerales.AsNoTracking().FirstAsync(x => x.Id == cedulon.PlanGeneralColegioId.Value, ct);
            var tipo = await db.TiposComprobante.AsNoTracking().FirstAsync(x => x.Id == plan.TipoComprobanteId, ct);
            var item = await db.Items.AsNoTracking().FirstAsync(x => x.Id == plan.ItemId, ct);
            var alicuota = await db.AlicuotasIva.AsNoTracking().FirstAsync(x => x.Id == item.AlicuotaIvaId, ct);

            short prefijo = 0;
            long numero = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + cedulon.Id;
            if (request.PuntoFacturacionId.HasValue)
            {
                var punto = await db.PuntosFacturacion.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.PuntoFacturacionId.Value, ct)
                    ?? throw new InvalidOperationException($"No se encontró el punto de facturación ID {request.PuntoFacturacionId.Value}.");
                prefijo = punto.Numero;
                numero = await comprobanteRepo.GetProximoNumeroAsync(request.PuntoFacturacionId.Value, plan.TipoComprobanteId, ct);
            }

            var comprobante = Comprobante.Crear(
                plan.SucursalId,
                request.PuntoFacturacionId,
                plan.TipoComprobanteId,
                prefijo,
                numero,
                request.Fecha,
                request.FechaVencimiento ?? cedulon.FechaVencimiento,
                cedulon.TerceroId,
                plan.MonedaId,
                1m,
                request.Observacion ?? $"Facturación colegio - {cedulon.NroCedulon}",
                currentUser.UserId);

            comprobante.AgregarItem(ComprobanteItem.Crear(
                0,
                item.Id,
                plan.Descripcion,
                1m,
                0,
                (long)Math.Round(cedulon.Importe, 0, MidpointRounding.AwayFromZero),
                0,
                alicuota.Id,
                alicuota.Porcentaje,
                null,
                0,
                false));

            comprobante.Emitir(currentUser.UserId);
            await comprobanteRepo.AddAsync(comprobante, ct);
            await db.SaveChangesAsync(ct);

            await circuitoVentas.AplicarEfectosAsync(comprobante, tipo, OperacionStockVenta.Ninguna, OperacionCuentaCorrienteVenta.Debito, ct);
            cedulon.AsociarComprobante(comprobante.Id, currentUser.UserId);
            cedulonRepo.Update(cedulon);
            ids.Add(comprobante.Id);
        }

        var loteIds = cedulones.Where(x => x.LoteColegioId.HasValue).Select(x => x.LoteColegioId!.Value).Distinct().ToList();
        foreach (var loteId in loteIds)
        {
            var pendientes = await db.Cedulones.AsNoTracking().AnyAsync(x => x.LoteColegioId == loteId && !x.ComprobanteId.HasValue, ct);
            if (!pendientes)
            {
                var lote = await db.ColegioLotes.FirstOrDefaultAsync(x => x.Id == loteId, ct);
                if (lote is not null)
                {
                    lote.MarcarFacturado(currentUser.UserId);
                    loteRepo.Update(lote);
                }
            }
        }

        return ids;
    }

    public async Task<IReadOnlyList<long>> FacturarCedulonesAutomaticoAsync(FacturarCedulonesColegioAutomaticoCommand request, CancellationToken ct)
    {
        var query = db.Cedulones.AsNoTracking()
            .Where(x => x.PlanGeneralColegioId.HasValue && !x.ComprobanteId.HasValue && !x.IsDeleted);

        if (request.LoteId.HasValue)
            query = query.Where(x => x.LoteColegioId == request.LoteId.Value);
        if (request.SucursalId.HasValue)
            query = query.Where(x => x.SucursalId == request.SucursalId.Value);
        if (request.HastaVencimiento.HasValue)
            query = query.Where(x => x.FechaVencimiento <= request.HastaVencimiento.Value);

        var ids = await query.OrderBy(x => x.FechaVencimiento).ThenBy(x => x.Id).Select(x => x.Id).ToListAsync(ct);
        return await FacturarCedulonesAsync(new FacturarCedulonesColegioCommand(ids, request.PuntoFacturacionId, request.Fecha, request.FechaVencimiento, request.Observacion), ct);
    }

    public async Task CerrarLoteAsync(long loteId, string? observacion, CancellationToken ct)
    {
        var lote = await db.ColegioLotes.FirstOrDefaultAsync(x => x.Id == loteId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró el lote de colegio ID {loteId}.");

        var cedulones = await db.Cedulones.AsNoTracking().Where(x => x.LoteColegioId == loteId && !x.IsDeleted).ToListAsync(ct);
        if (cedulones.Count == 0)
            throw new InvalidOperationException("No se puede cerrar un lote sin cedulones emitidos.");
        if (cedulones.Any(x => !x.ComprobanteId.HasValue && x.Estado != EstadoCedulon.Pagado))
            throw new InvalidOperationException("No se puede cerrar el lote con cedulones pendientes sin facturar o cancelar.");

        lote.Cerrar(observacion, currentUser.UserId);
        loteRepo.Update(lote);
    }

    public async Task<int> VencerCedulonesAsync(long? loteId, long? sucursalId, DateOnly fechaCorte, CancellationToken ct)
    {
        var query = db.Cedulones.Where(x => !x.IsDeleted
            && x.Estado != EstadoCedulon.Pagado
            && x.Estado != EstadoCedulon.Anulado
            && x.FechaVencimiento < fechaCorte);

        if (loteId.HasValue)
            query = query.Where(x => x.LoteColegioId == loteId.Value);
        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        var cedulones = await query.ToListAsync(ct);
        foreach (var cedulon in cedulones)
            cedulon.Vencer(currentUser.UserId);

        return cedulones.Count;
    }

    public async Task<ReporteTabularDto> GetReporteCedulonesAsync(long? loteId, long? planGeneralColegioId, bool soloPendientes, CancellationToken ct)
    {
        var query = db.Cedulones.AsNoTracking().Where(x => !x.IsDeleted && x.PlanGeneralColegioId.HasValue);
        if (loteId.HasValue)
            query = query.Where(x => x.LoteColegioId == loteId.Value);
        if (planGeneralColegioId.HasValue)
            query = query.Where(x => x.PlanGeneralColegioId == planGeneralColegioId.Value);
        if (soloPendientes)
            query = query.Where(x => x.ImportePagado < x.Importe);

        var items = await query.OrderBy(x => x.FechaVencimiento).ThenBy(x => x.Id).ToListAsync(ct);
        return new ReporteTabularDto
        {
            Titulo = "Cedulones Colegio",
            Parametros = new Dictionary<string, string>
            {
                ["LoteId"] = loteId?.ToString() ?? "Todos",
                ["PlanGeneralColegioId"] = planGeneralColegioId?.ToString() ?? "Todos",
                ["SoloPendientes"] = soloPendientes ? "SI" : "NO"
            },
            Columnas = ["Id", "NroCedulon", "TerceroId", "LoteId", "Emision", "Vencimiento", "Importe", "Pagado", "Saldo", "Estado", "ComprobanteId"],
            Filas = items.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Id.ToString(),
                x.NroCedulon,
                x.TerceroId.ToString(),
                x.LoteColegioId?.ToString() ?? "—",
                x.FechaEmision.ToString("yyyy-MM-dd"),
                x.FechaVencimiento.ToString("yyyy-MM-dd"),
                x.Importe.ToString("0.00"),
                x.ImportePagado.ToString("0.00"),
                (x.Importe - x.ImportePagado).ToString("0.00"),
                x.Estado.ToString().ToUpperInvariant(),
                x.ComprobanteId?.ToString() ?? "—"
            }).ToList().AsReadOnly()
        };
    }

    public async Task<ReporteTabularDto> GetReporteRecibosAsync(long? terceroId, DateOnly? desde, DateOnly? hasta, CancellationToken ct)
    {
        var query = db.Cobros.AsNoTracking().Where(x => !x.IsDeleted && x.Observacion != null && x.Observacion.Contains("COLEGIO"));
        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);
        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);

        var cobros = await query.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        var detalles = await db.ColegioRecibosDetalles.AsNoTracking().Where(x => cobros.Select(c => c.Id).Contains(x.CobroId)).ToListAsync(ct);

        return new ReporteTabularDto
        {
            Titulo = "Recibos Colegio",
            Parametros = new Dictionary<string, string>
            {
                ["TerceroId"] = terceroId?.ToString() ?? "Todos",
                ["Desde"] = desde?.ToString("yyyy-MM-dd") ?? "—",
                ["Hasta"] = hasta?.ToString("yyyy-MM-dd") ?? "—"
            },
            Columnas = ["Id", "Fecha", "TerceroId", "Total", "Cedulones", "Observacion"],
            Filas = cobros.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Id.ToString(),
                x.Fecha.ToString("yyyy-MM-dd"),
                x.TerceroId.ToString(),
                x.Total.ToString("0.00"),
                detalles.Count(d => d.CobroId == x.Id).ToString(),
                x.Observacion ?? "—"
            }).ToList().AsReadOnly()
        };
    }

    private async Task<long> RegistrarCobroColegioAsync(long sucursalId, long terceroId, DateOnly fecha, long monedaId, decimal cotizacion, long cajaId, long formaPagoId, string? observacion, IReadOnlyList<CancelacionCedulonColegioInput> cedulonesInput, CancellationToken ct)
    {
        var cedulones = await db.Cedulones.Where(x => cedulonesInput.Select(c => c.CedulonId).Contains(x.Id)).ToListAsync(ct);
        if (cedulones.Count != cedulonesInput.Count)
            throw new InvalidOperationException("Uno o más cedulones a cancelar no existen.");
        if (cedulonesInput.Select(x => x.CedulonId).Distinct().Count() != cedulonesInput.Count)
            throw new InvalidOperationException("No se puede repetir el mismo cedulón dentro del mismo recibo.");

        if (cedulones.Any(x => x.TerceroId != terceroId || x.SucursalId != sucursalId))
            throw new InvalidOperationException("Todos los cedulones del recibo deben pertenecer al mismo tercero y sucursal.");
        if (cedulones.Any(x => x.Estado == EstadoCedulon.Pagado))
            throw new InvalidOperationException("No se pueden volver a cobrar cedulones totalmente pagados.");

        foreach (var item in cedulonesInput)
        {
            var cedulon = cedulones.First(x => x.Id == item.CedulonId);
            var saldo = cedulon.Importe - cedulon.ImportePagado;
            if (item.Importe <= 0)
                throw new InvalidOperationException($"El importe a cobrar del cedulón {cedulon.NroCedulon} debe ser mayor a 0.");
            if (item.Importe > saldo)
                throw new InvalidOperationException($"El importe a cobrar del cedulón {cedulon.NroCedulon} excede el saldo pendiente.");
        }

        var total = cedulonesInput.Sum(x => x.Importe);
        var cobro = Cobro.Crear(sucursalId, terceroId, fecha, monedaId, cotizacion, $"COLEGIO | {observacion}".Trim(), currentUser.UserId);
        cobro.AgregarMedio(CobroMedio.Crear(0, cajaId, formaPagoId, null, total, monedaId, cotizacion));
        await cobroRepo.AddAsync(cobro, ct);
        await db.SaveChangesAsync(ct);

        foreach (var item in cedulonesInput)
        {
            var cedulon = cedulones.First(x => x.Id == item.CedulonId);
            cedulon.RegistrarPago(item.Importe, currentUser.UserId);
            cedulonRepo.Update(cedulon);
            await reciboDetalleRepo.AddAsync(ColegioReciboDetalle.Crear(cobro.Id, cedulon.Id, item.Importe), ct);
        }

        await tesoreriaService.RegistrarMovimientoAsync(
            sucursalId,
            cajaId,
            fecha,
            TipoOperacionTesoreria.CobroVentanilla,
            SentidoMovimientoTesoreria.Ingreso,
            total,
            monedaId,
            cotizacion,
            terceroId,
            "COLEGIO",
            cobro.Id,
            observacion,
            ct);

        return cobro.Id;
    }

    private async Task ValidarReferenciasPlanGeneralAsync(long planPagoId, long tipoComprobanteId, long itemId, long monedaId, CancellationToken ct)
    {
        if (!await db.PlanesPago.AsNoTracking().AnyAsync(x => x.Id == planPagoId && x.Activo, ct))
            throw new InvalidOperationException($"No se encontró el plan de pago ID {planPagoId}.");
        if (!await db.TiposComprobante.AsNoTracking().AnyAsync(x => x.Id == tipoComprobanteId && x.Activo, ct))
            throw new InvalidOperationException($"No se encontró el tipo de comprobante ID {tipoComprobanteId}.");
        if (!await db.Items.AsNoTracking().AnyAsync(x => x.Id == itemId && x.Activo, ct))
            throw new InvalidOperationException($"No se encontró el ítem ID {itemId}.");
        if (!await db.Monedas.AsNoTracking().AnyAsync(x => x.Id == monedaId, ct))
            throw new InvalidOperationException($"No se encontró la moneda ID {monedaId}.");
    }
}
