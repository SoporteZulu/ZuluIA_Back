using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Reportes.Services;

public class ReportesService(IApplicationDbContext db)
{
    public async Task<ReporteTabularDto> GetReporteRemitosAsync(long? sucursalId, DateOnly desde, DateOnly hasta, bool? esVenta, CancellationToken ct)
    {
        var tipos = db.TiposComprobante.AsNoTracking()
            .Where(x => (x.Codigo.ToUpper().Contains("REMIT") || x.Descripcion.ToUpper().Contains("REMIT")) && x.Activo);

        if (esVenta.HasValue)
            tipos = esVenta.Value ? tipos.Where(x => x.EsVenta) : tipos.Where(x => x.EsCompra);

        var tipoIds = await tipos.Select(x => x.Id).ToListAsync(ct);
        var query = db.Comprobantes.AsNoTracking()
            .Where(x => tipoIds.Contains(x.TipoComprobanteId) && x.Fecha >= desde && x.Fecha <= hasta);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        var comprobantes = await query
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.TipoComprobanteId,
                Numero = x.Numero.Formateado,
                x.Fecha,
                x.TerceroId,
                x.Total,
                x.Estado,
                x.ComprobanteOrigenId
            })
            .ToListAsync(ct);

        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => comprobantes.Select(c => c.TerceroId).Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, ct);

        var tiposLookup = await db.TiposComprobante.AsNoTracking()
            .Where(x => comprobantes.Select(c => c.TipoComprobanteId).Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        return new ReporteTabularDto
        {
            Titulo = "Reporte de Remitos",
            Parametros = new Dictionary<string, string>
            {
                ["Desde"] = desde.ToString("yyyy-MM-dd"),
                ["Hasta"] = hasta.ToString("yyyy-MM-dd"),
                ["SucursalId"] = sucursalId?.ToString() ?? "Todas"
            },
            Columnas = ["Id", "Fecha", "Tipo", "Número", "Tercero", "Total", "Estado", "Origen"],
            Filas = comprobantes.Select(x => (IReadOnlyList<string>)
            [
                x.Id.ToString(),
                x.Fecha.ToString("yyyy-MM-dd"),
                tiposLookup.GetValueOrDefault(x.TipoComprobanteId)?.Descripcion ?? "—",
                x.Numero,
                terceros.GetValueOrDefault(x.TerceroId)?.RazonSocial ?? "—",
                x.Total.ToString("0.00"),
                x.Estado.ToString().ToUpperInvariant(),
                x.ComprobanteOrigenId?.ToString() ?? "—"
            ]).ToList().AsReadOnly()
        };
    }

    public async Task<ReporteTabularDto> GetInformeContableAsync(long ejercicioId, DateOnly desde, DateOnly hasta, long? sucursalId, CancellationToken ct)
    {
        var asientos = db.Asientos.AsNoTracking()
            .Where(x => x.EjercicioId == ejercicioId && x.Fecha >= desde && x.Fecha <= hasta && x.Estado != EstadoAsiento.Anulado);

        if (sucursalId.HasValue)
            asientos = asientos.Where(x => x.SucursalId == sucursalId.Value);

        var asientoIds = await asientos.Select(x => x.Id).ToListAsync(ct);
        var movimientos = await db.AsientosLineas.AsNoTracking()
            .Where(x => asientoIds.Contains(x.AsientoId))
            .GroupBy(x => x.CuentaId)
            .Select(g => new { CuentaId = g.Key, Debe = g.Sum(x => x.Debe), Haber = g.Sum(x => x.Haber) })
            .ToListAsync(ct);

        var cuentas = await db.PlanCuentas.AsNoTracking()
            .Where(x => movimientos.Select(m => m.CuentaId).Contains(x.Id))
            .Select(x => new { x.Id, x.CodigoCuenta, x.Denominacion })
            .ToDictionaryAsync(x => x.Id, ct);

        return new ReporteTabularDto
        {
            Titulo = "Informe Contable",
            Parametros = new Dictionary<string, string>
            {
                ["EjercicioId"] = ejercicioId.ToString(),
                ["Desde"] = desde.ToString("yyyy-MM-dd"),
                ["Hasta"] = hasta.ToString("yyyy-MM-dd")
            },
            Columnas = ["Cuenta", "Denominación", "Debe", "Haber", "Saldo"],
            Filas = movimientos.OrderBy(x => cuentas.GetValueOrDefault(x.CuentaId)?.CodigoCuenta).Select(x => (IReadOnlyList<string>)
            [
                cuentas.GetValueOrDefault(x.CuentaId)?.CodigoCuenta ?? "—",
                cuentas.GetValueOrDefault(x.CuentaId)?.Denominacion ?? "—",
                x.Debe.ToString("0.00"),
                x.Haber.ToString("0.00"),
                (x.Debe - x.Haber).ToString("0.00")
            ]).ToList().AsReadOnly()
        };
    }

    public async Task<ReporteTabularDto> GetInformeOperativoAsync(long sucursalId, DateOnly desde, DateOnly hasta, long? depositoId, CancellationToken ct)
    {
        var depositos = db.Depositos.AsNoTracking().Where(x => x.SucursalId == sucursalId);
        if (depositoId.HasValue)
            depositos = depositos.Where(x => x.Id == depositoId.Value);

        var depositoIds = await depositos.Select(x => x.Id).ToListAsync(ct);
        var movimientos = await db.MovimientosStock.AsNoTracking()
            .Where(x => depositoIds.Contains(x.DepositoId) && x.Fecha >= desde.ToDateTime(TimeOnly.MinValue) && x.Fecha <= hasta.ToDateTime(TimeOnly.MaxValue))
            .GroupBy(x => new { x.ItemId, x.DepositoId, x.TipoMovimiento })
            .Select(g => new { g.Key.ItemId, g.Key.DepositoId, g.Key.TipoMovimiento, Cantidad = g.Sum(x => x.Cantidad) })
            .ToListAsync(ct);

        var items = await db.Items.AsNoTracking()
            .Where(x => movimientos.Select(m => m.ItemId).Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var depositosLookup = await db.Depositos.AsNoTracking()
            .Where(x => movimientos.Select(m => m.DepositoId).Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        return new ReporteTabularDto
        {
            Titulo = "Informe Operativo de Stock",
            Parametros = new Dictionary<string, string>
            {
                ["SucursalId"] = sucursalId.ToString(),
                ["Desde"] = desde.ToString("yyyy-MM-dd"),
                ["Hasta"] = hasta.ToString("yyyy-MM-dd")
            },
            Columnas = ["Depósito", "Item", "Descripción", "Tipo Movimiento", "Cantidad"],
            Filas = movimientos.OrderBy(x => depositosLookup.GetValueOrDefault(x.DepositoId)?.Descripcion)
                .ThenBy(x => items.GetValueOrDefault(x.ItemId)?.Codigo)
                .Select(x => (IReadOnlyList<string>)
                [
                    depositosLookup.GetValueOrDefault(x.DepositoId)?.Descripcion ?? "—",
                    items.GetValueOrDefault(x.ItemId)?.Codigo ?? "—",
                    items.GetValueOrDefault(x.ItemId)?.Descripcion ?? "—",
                    x.TipoMovimiento.ToString().ToUpperInvariant(),
                    x.Cantidad.ToString("0.####")
                ]).ToList().AsReadOnly()
        };
    }

    public async Task<DashboardReporteDto> GetDashboardComercialAsync(long sucursalId, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var comprobantes = await db.Comprobantes.AsNoTracking()
            .Where(x => x.SucursalId == sucursalId && x.Fecha >= desde && x.Fecha <= hasta)
            .ToListAsync(ct);

        var ventas = comprobantes.Where(x => x.Total >= 0).ToList();
        var compras = comprobantes.Where(x => x.Total < 0).ToList();

        return new DashboardReporteDto
        {
            Titulo = "Dashboard Comercial",
            Indicadores = new Dictionary<string, decimal>
            {
                ["CantidadComprobantes"] = comprobantes.Count,
                ["TotalVentas"] = ventas.Sum(x => x.Total),
                ["TotalCompras"] = Math.Abs(compras.Sum(x => x.Total)),
                ["SaldoPendiente"] = comprobantes.Sum(x => x.Saldo)
            },
            Series = comprobantes.GroupBy(x => x.Fecha)
                .OrderBy(x => x.Key)
                .Select(x => new DashboardSerieDto
                {
                    Etiqueta = x.Key.ToString("yyyy-MM-dd"),
                    Valor = x.Sum(v => v.Total)
                })
                .ToList()
                .AsReadOnly()
        };
    }

    public async Task<DashboardReporteDto> GetDashboardOperativoAsync(long sucursalId, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var depositoIds = await db.Depositos.AsNoTracking()
            .Where(x => x.SucursalId == sucursalId)
            .Select(x => x.Id)
            .ToListAsync(ct);

        var desdeFecha = new DateTimeOffset(desde.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var hastaFecha = new DateTimeOffset(hasta.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var movimientos = await db.MovimientosStock.AsNoTracking()
            .Where(x => depositoIds.Contains(x.DepositoId) && x.Fecha >= desdeFecha && x.Fecha <= hastaFecha)
            .ToListAsync(ct);

        return new DashboardReporteDto
        {
            Titulo = "Dashboard Operativo",
            Indicadores = new Dictionary<string, decimal>
            {
                ["CantidadMovimientos"] = movimientos.Count,
                ["Ingresos"] = movimientos.Where(x => x.Cantidad > 0).Sum(x => x.Cantidad),
                ["Egresos"] = Math.Abs(movimientos.Where(x => x.Cantidad < 0).Sum(x => x.Cantidad)),
                ["CantidadTiposMovimiento"] = movimientos.Select(x => x.TipoMovimiento).Distinct().Count()
            },
            Series = movimientos.GroupBy(x => x.TipoMovimiento)
                .OrderBy(x => x.Key.ToString())
                .Select(x => new DashboardSerieDto
                {
                    Etiqueta = x.Key.ToString().ToUpperInvariant(),
                    Valor = x.Sum(v => Math.Abs(v.Cantidad))
                })
                .ToList()
                .AsReadOnly()
        };
    }

    public Task<ReporteTabularDto> GetReporteParametrizadoAsync(TipoReporteParametrizado tipo, long? sucursalId, long? ejercicioId, DateOnly desde, DateOnly hasta, long? depositoId, CancellationToken ct)
    {
        return tipo switch
        {
            TipoReporteParametrizado.Remitos => GetReporteRemitosAsync(sucursalId, desde, hasta, true, ct),
            TipoReporteParametrizado.Contable => GetInformeContableAsync(ejercicioId ?? throw new InvalidOperationException("El ejercicioId es obligatorio para el reporte contable."), desde, hasta, sucursalId, ct),
            _ => GetInformeOperativoAsync(sucursalId ?? throw new InvalidOperationException("La sucursalId es obligatoria para el reporte operativo."), desde, hasta, depositoId, ct)
        };
    }
}
