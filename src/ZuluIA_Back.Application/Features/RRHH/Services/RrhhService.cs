using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.RRHH.Commands;
using ZuluIA_Back.Application.Features.RRHH.DTOs;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;
using ZuluIA_Back.Application.Features.Tesoreria.Services;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.RRHH.Services;

public class RrhhService(
    IApplicationDbContext db,
    IEmpleadoRepository empleadoRepo,
    IRepository<ComprobanteEmpleado> comprobanteRepo,
    IRepository<ImputacionEmpleado> imputacionRepo,
    IRepository<LiquidacionSueldo> liquidacionRepo,
    TesoreriaService tesoreriaService,
    ReporteExportacionService exportacionService,
    ICurrentUserService currentUser)
{
    public async Task ActualizarEmpleadoAsync(UpdateEmpleadoCommand request, CancellationToken ct)
    {
        var empleado = await empleadoRepo.GetByIdAsync(request.Id, ct)
            ?? throw new InvalidOperationException($"No se encontró el empleado ID {request.Id}.");

        if (!await db.Monedas.AsNoTracking().AnyAsync(x => x.Id == request.MonedaId, ct))
            throw new InvalidOperationException($"No se encontró la moneda ID {request.MonedaId}.");

        empleado.Actualizar(request.Cargo, request.Area, request.SueldoBasico, request.MonedaId);
        empleadoRepo.Update(empleado);
    }

    public async Task CambiarEstadoEmpleadoAsync(CambiarEstadoEmpleadoCommand request, CancellationToken ct)
    {
        var empleado = await empleadoRepo.GetByIdAsync(request.Id, ct)
            ?? throw new InvalidOperationException($"No se encontró el empleado ID {request.Id}.");

        switch (request.Estado)
        {
            case EstadoEmpleado.Activo:
                empleado.Reactivar();
                break;
            case EstadoEmpleado.Suspendido:
                empleado.Suspender();
                break;
            case EstadoEmpleado.Inactivo:
                empleado.Egresar(request.FechaEgreso ?? throw new InvalidOperationException("La fecha de egreso es obligatoria."));
                break;
        }

        empleadoRepo.Update(empleado);
    }

    public async Task ActualizarLiquidacionAsync(UpdateLiquidacionSueldoCommand request, CancellationToken ct)
    {
        var liquidacion = await db.LiquidacionesSueldo.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new InvalidOperationException($"No se encontró la liquidación ID {request.Id}.");

        liquidacion.Actualizar(request.SueldoBasico, request.TotalHaberes, request.TotalDescuentos, request.MonedaId, request.Observacion);
        liquidacionRepo.Update(liquidacion);
    }

    public async Task<IReadOnlyList<long>> GenerarLiquidacionesMasivasAsync(GenerarLiquidacionesMasivasCommand request, CancellationToken ct)
    {
        var empleadosQuery = db.Empleados.AsNoTracking()
            .Where(x => x.SucursalId == request.SucursalId && x.Estado == EstadoEmpleado.Activo);

        if (request.EmpleadoIds is { Count: > 0 })
            empleadosQuery = empleadosQuery.Where(x => request.EmpleadoIds.Contains(x.Id));

        var empleados = await empleadosQuery.OrderBy(x => x.Legajo).ToListAsync(ct);
        if (empleados.Count == 0)
            throw new InvalidOperationException("No se encontraron empleados activos para generar liquidaciones.");

        var existentes = await db.LiquidacionesSueldo.AsNoTracking()
            .Where(x => x.SucursalId == request.SucursalId && x.Anio == request.Anio && x.Mes == request.Mes)
            .Select(x => x.EmpleadoId)
            .ToListAsync(ct);

        var nuevasLiquidaciones = new List<LiquidacionSueldo>();
        foreach (var empleado in empleados.Where(x => !existentes.Contains(x.Id)))
        {
            var sueldoBasico = request.PorcentajeAjuste.HasValue
                ? decimal.Round(empleado.SueldoBasico * (1 + (request.PorcentajeAjuste.Value / 100m)), 4, MidpointRounding.AwayFromZero)
                : empleado.SueldoBasico;

            var liquidacion = LiquidacionSueldo.Crear(
                empleado.Id,
                empleado.SucursalId,
                request.Anio,
                request.Mes,
                sueldoBasico,
                sueldoBasico,
                0m,
                empleado.MonedaId,
                request.Observacion);

            await db.LiquidacionesSueldo.AddAsync(liquidacion, ct);
            nuevasLiquidaciones.Add(liquidacion);
        }

        await db.SaveChangesAsync(ct);

        return nuevasLiquidaciones.Select(x => x.Id).ToList().AsReadOnly();
    }

    public async Task<ComprobanteEmpleado> GenerarComprobanteEmpleadoAsync(Commands.GenerarComprobanteEmpleadoCommand request, CancellationToken ct)
    {
        var liquidacion = await db.LiquidacionesSueldo.FirstOrDefaultAsync(x => x.Id == request.LiquidacionSueldoId, ct)
            ?? throw new InvalidOperationException($"No se encontró la liquidación ID {request.LiquidacionSueldoId}.");

        if (liquidacion.ComprobanteEmpleadoId.HasValue)
            throw new InvalidOperationException("La liquidación ya tiene un comprobante de empleado asociado.");

        var empleado = await db.Empleados.AsNoTracking().FirstOrDefaultAsync(x => x.Id == liquidacion.EmpleadoId, ct)
            ?? throw new InvalidOperationException($"No se encontró el empleado ID {liquidacion.EmpleadoId}.");

        var numero = $"REC-{empleado.Legajo}-{liquidacion.Anio}{liquidacion.Mes:D2}";
        var comprobante = ComprobanteEmpleado.Crear(empleado.Id, liquidacion.Id, liquidacion.SucursalId, request.Fecha, request.Tipo, numero, liquidacion.Neto, liquidacion.MonedaId, request.Observacion, currentUser.UserId);
        await comprobanteRepo.AddAsync(comprobante, ct);
        liquidacion.AsociarComprobanteEmpleado(comprobante.Id);
        liquidacionRepo.Update(liquidacion);
        return comprobante;
    }

    public async Task<ImputacionEmpleado> ImputarLiquidacionAsync(Commands.ImputarLiquidacionEmpleadoCommand request, CancellationToken ct)
    {
        var liquidacion = await db.LiquidacionesSueldo.FirstOrDefaultAsync(x => x.Id == request.LiquidacionSueldoId, ct)
            ?? throw new InvalidOperationException($"No se encontró la liquidación ID {request.LiquidacionSueldoId}.");

        var empleado = await db.Empleados.AsNoTracking().FirstOrDefaultAsync(x => x.Id == liquidacion.EmpleadoId, ct)
            ?? throw new InvalidOperationException($"No se encontró el empleado ID {liquidacion.EmpleadoId}.");

        if (request.Importe > liquidacion.SaldoPendiente)
            throw new InvalidOperationException("El importe a imputar supera el saldo pendiente de la liquidación.");

        ComprobanteEmpleado? comprobante = null;
        if (liquidacion.ComprobanteEmpleadoId.HasValue)
        {
            comprobante = await db.ComprobantesEmpleados.AsNoTracking().FirstOrDefaultAsync(x => x.Id == liquidacion.ComprobanteEmpleadoId.Value, ct);
        }
        else if (request.GenerarComprobanteSiNoExiste)
        {
            comprobante = await GenerarComprobanteEmpleadoAsync(new Commands.GenerarComprobanteEmpleadoCommand(liquidacion.Id, request.Fecha, "RECIBO_SUELDO", request.Observacion), ct);
        }

        var movimiento = await tesoreriaService.RegistrarMovimientoAsync(
            liquidacion.SucursalId,
            request.CajaId,
            request.Fecha,
            TipoOperacionTesoreria.PagoVentanilla,
            SentidoMovimientoTesoreria.Egreso,
            request.Importe,
            liquidacion.MonedaId,
            1m,
            empleado.TerceroId,
            "RRHH",
            liquidacion.Id,
            request.Observacion,
            ct);

        liquidacion.RegistrarImputacion(request.Importe, request.Fecha);
        liquidacionRepo.Update(liquidacion);

        var imputacion = ImputacionEmpleado.Registrar(liquidacion.Id, comprobante?.Id ?? liquidacion.ComprobanteEmpleadoId, movimiento.Id, request.Fecha, request.Importe, request.Observacion, currentUser.UserId);
        await imputacionRepo.AddAsync(imputacion, ct);
        return imputacion;
    }

    public async Task<ExportacionReporteDto> GenerarPdfComprobanteEmpleadoAsync(long comprobanteEmpleadoId, CancellationToken ct)
    {
        var comprobante = await db.ComprobantesEmpleados.AsNoTracking().FirstOrDefaultAsync(x => x.Id == comprobanteEmpleadoId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró el comprobante de empleado ID {comprobanteEmpleadoId}.");

        var empleado = await db.Empleados.AsNoTracking().FirstOrDefaultAsync(x => x.Id == comprobante.EmpleadoId, ct)
            ?? throw new InvalidOperationException($"No se encontró el empleado ID {comprobante.EmpleadoId}.");

        var tercero = await db.Terceros.AsNoTracking().Where(x => x.Id == empleado.TerceroId).Select(x => new { x.RazonSocial, x.NroDocumento }).FirstOrDefaultAsync(ct);
        var reporte = new ReporteTabularDto
        {
            Titulo = $"Comprobante Empleado {comprobante.Numero}",
            Parametros = new Dictionary<string, string>
            {
                ["Legajo"] = empleado.Legajo,
                ["Empleado"] = tercero?.RazonSocial ?? "—",
                ["CUIT"] = tercero?.NroDocumento ?? "—",
                ["Fecha"] = comprobante.Fecha.ToString("yyyy-MM-dd"),
                ["Tipo"] = comprobante.Tipo,
                ["Total"] = comprobante.Total.ToString("0.00")
            },
            Columnas = ["Concepto", "Importe"],
            Filas = [new[] { "Neto liquidación", comprobante.Total.ToString("0.00") }]
        };

        return exportacionService.Exportar(reporte, FormatoExportacionReporte.Pdf, $"comprobante_empleado_{comprobante.Id}");
    }

    public async Task<ReporteTabularDto> GetReporteEmpleadosAsync(long? sucursalId, EstadoEmpleado? estado, CancellationToken ct)
    {
        var query = db.Empleados.AsNoTracking();
        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (estado.HasValue)
            query = query.Where(x => x.Estado == estado.Value);

        var empleados = await query.OrderBy(x => x.Legajo).ToListAsync(ct);
        var terceroIds = empleados.Select(x => x.TerceroId).Distinct().ToList();
        var terceros = await db.Terceros.AsNoTracking().Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial, x.NroDocumento })
            .ToDictionaryAsync(x => x.Id, ct);

        return new ReporteTabularDto
        {
            Titulo = "Empleados",
            Parametros = new Dictionary<string, string>
            {
                ["SucursalId"] = sucursalId?.ToString() ?? "Todas",
                ["Estado"] = estado?.ToString().ToUpperInvariant() ?? "Todos"
            },
            Columnas = ["Legajo", "Empleado", "CUIT", "Cargo", "Area", "Ingreso", "Sueldo", "Estado"],
            Filas = empleados.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Legajo,
                terceros.GetValueOrDefault(x.TerceroId)?.RazonSocial ?? "—",
                terceros.GetValueOrDefault(x.TerceroId)?.NroDocumento ?? "—",
                x.Cargo,
                x.Area ?? "—",
                x.FechaIngreso.ToString("yyyy-MM-dd"),
                x.SueldoBasico.ToString("0.00"),
                x.Estado.ToString().ToUpperInvariant()
            }).ToList().AsReadOnly()
        };
    }

    public async Task<ReporteTabularDto> GetReporteLiquidacionesAsync(long? empleadoId, bool? pagada, int? anio, int? mes, CancellationToken ct)
    {
        var query = db.LiquidacionesSueldo.AsNoTracking();
        if (empleadoId.HasValue)
            query = query.Where(x => x.EmpleadoId == empleadoId.Value);
        if (pagada.HasValue)
            query = query.Where(x => x.Pagada == pagada.Value);
        if (anio.HasValue)
            query = query.Where(x => x.Anio == anio.Value);
        if (mes.HasValue)
            query = query.Where(x => x.Mes == mes.Value);

        var liquidaciones = await query.OrderByDescending(x => x.Anio).ThenByDescending(x => x.Mes).ThenByDescending(x => x.Id).ToListAsync(ct);
        var empleadoIds = liquidaciones.Select(x => x.EmpleadoId).Distinct().ToList();
        var empleados = await db.Empleados.AsNoTracking().Where(x => empleadoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Legajo })
            .ToDictionaryAsync(x => x.Id, ct);

        return new ReporteTabularDto
        {
            Titulo = "Liquidaciones de Sueldo",
            Parametros = new Dictionary<string, string>
            {
                ["EmpleadoId"] = empleadoId?.ToString() ?? "Todos",
                ["Pagada"] = pagada.HasValue ? (pagada.Value ? "SI" : "NO") : "Todos",
                ["Anio"] = anio?.ToString() ?? "Todos",
                ["Mes"] = mes?.ToString() ?? "Todos"
            },
            Columnas = ["Id", "Legajo", "Periodo", "Haberes", "Descuentos", "Neto", "Imputado", "Saldo", "Pagada"],
            Filas = liquidaciones.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Id.ToString(),
                empleados.GetValueOrDefault(x.EmpleadoId)?.Legajo ?? "—",
                $"{x.Anio}/{x.Mes:D2}",
                x.TotalHaberes.ToString("0.00"),
                x.TotalDescuentos.ToString("0.00"),
                x.Neto.ToString("0.00"),
                x.ImporteImputado.ToString("0.00"),
                (x.Neto - x.ImporteImputado).ToString("0.00"),
                x.Pagada ? "SI" : "NO"
            }).ToList().AsReadOnly()
        };
    }

    public async Task<ReporteTabularDto> GetReporteComprobantesAsync(long? empleadoId, long? liquidacionSueldoId, string? tipo, CancellationToken ct)
    {
        var query = db.ComprobantesEmpleados.AsNoTracking().Where(x => !x.IsDeleted);
        if (empleadoId.HasValue)
            query = query.Where(x => x.EmpleadoId == empleadoId.Value);
        if (liquidacionSueldoId.HasValue)
            query = query.Where(x => x.LiquidacionSueldoId == liquidacionSueldoId.Value);
        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var tipoNormalizado = tipo.Trim().ToUpperInvariant();
            query = query.Where(x => x.Tipo == tipoNormalizado);
        }

        var comprobantes = await query.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        var empleadoIds = comprobantes.Select(x => x.EmpleadoId).Distinct().ToList();
        var empleados = await db.Empleados.AsNoTracking()
            .Where(x => empleadoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Legajo })
            .ToDictionaryAsync(x => x.Id, ct);

        return new ReporteTabularDto
        {
            Titulo = "Comprobantes de Empleados",
            Parametros = new Dictionary<string, string>
            {
                ["EmpleadoId"] = empleadoId?.ToString() ?? "Todos",
                ["LiquidacionSueldoId"] = liquidacionSueldoId?.ToString() ?? "Todas",
                ["Tipo"] = string.IsNullOrWhiteSpace(tipo) ? "Todos" : tipo.Trim().ToUpperInvariant()
            },
            Columnas = ["Id", "Legajo", "Liquidacion", "Fecha", "Tipo", "Numero", "Total"],
            Filas = comprobantes.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Id.ToString(),
                empleados.GetValueOrDefault(x.EmpleadoId)?.Legajo ?? "—",
                x.LiquidacionSueldoId.ToString(),
                x.Fecha.ToString("yyyy-MM-dd"),
                x.Tipo,
                x.Numero,
                x.Total.ToString("0.00")
            }).ToList().AsReadOnly()
        };
    }

    public async Task<ReporteTabularDto> GetReporteImputacionesAsync(long? empleadoId, long? liquidacionSueldoId, CancellationToken ct)
    {
        var query = from imputacion in db.ImputacionesEmpleados.AsNoTracking().Where(x => !x.IsDeleted)
                    join liquidacion in db.LiquidacionesSueldo.AsNoTracking() on imputacion.LiquidacionSueldoId equals liquidacion.Id
                    select new { imputacion, liquidacion };

        if (empleadoId.HasValue)
            query = query.Where(x => x.liquidacion.EmpleadoId == empleadoId.Value);
        if (liquidacionSueldoId.HasValue)
            query = query.Where(x => x.imputacion.LiquidacionSueldoId == liquidacionSueldoId.Value);

        var items = await query.OrderByDescending(x => x.imputacion.Fecha).ThenByDescending(x => x.imputacion.Id).ToListAsync(ct);
        var empleadoIds = items.Select(x => x.liquidacion.EmpleadoId).Distinct().ToList();
        var empleados = await db.Empleados.AsNoTracking()
            .Where(x => empleadoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Legajo })
            .ToDictionaryAsync(x => x.Id, ct);

        return new ReporteTabularDto
        {
            Titulo = "Imputaciones de Liquidaciones",
            Parametros = new Dictionary<string, string>
            {
                ["EmpleadoId"] = empleadoId?.ToString() ?? "Todos",
                ["LiquidacionSueldoId"] = liquidacionSueldoId?.ToString() ?? "Todas"
            },
            Columnas = ["Id", "Legajo", "Liquidacion", "Fecha", "Importe", "Comprobante", "TesoreriaMovimiento"],
            Filas = items.Select(x => (IReadOnlyList<string>)new[]
            {
                x.imputacion.Id.ToString(),
                empleados.GetValueOrDefault(x.liquidacion.EmpleadoId)?.Legajo ?? "—",
                x.imputacion.LiquidacionSueldoId.ToString(),
                x.imputacion.Fecha.ToString("yyyy-MM-dd"),
                x.imputacion.Importe.ToString("0.00"),
                x.imputacion.ComprobanteEmpleadoId?.ToString() ?? "—",
                x.imputacion.TesoreriaMovimientoId?.ToString() ?? "—"
            }).ToList().AsReadOnly()
        };
    }
}
