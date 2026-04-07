using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contratos.Commands;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Domain.Entities.Contratos;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Contratos.Services;

public class ContratosService(
    IApplicationDbContext db,
    IRepository<Contrato> contratoRepo,
    IRepository<ContratoHistorial> historialRepo,
    IRepository<ContratoImpacto> impactoRepo,
    CuentaCorrienteService cuentaCorrienteService,
    ICurrentUserService currentUser)
{
    public async Task<Contrato> CrearAsync(Commands.CreateContratoCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await db.Contratos.AsNoTracking().AnyAsync(x => x.Codigo == codigo, ct))
            throw new InvalidOperationException($"Ya existe un contrato con código '{request.Codigo}'.");

        await ValidarReferenciasAsync(request.TerceroId, request.SucursalId, request.MonedaId, ct);
        await ValidarSolapamientoAsync(null, request.TerceroId, request.SucursalId, request.FechaInicio, request.FechaFin, ct);

        var contrato = Contrato.Crear(request.TerceroId, request.SucursalId, request.MonedaId, request.Codigo, request.Descripcion, request.FechaInicio, request.FechaFin, request.Importe, request.RenovacionAutomatica, request.Observacion, currentUser.UserId);
        await contratoRepo.AddAsync(contrato, ct);
        await historialRepo.AddAsync(ContratoHistorial.Registrar(contrato.Id, TipoEventoContrato.Alta, request.FechaInicio, $"Alta de contrato {contrato.Codigo}.", contrato.Importe, currentUser.UserId), ct);
        await RegistrarImpactosAsync(contrato, request.FechaInicio, request.GenerarImpactoComercial, request.GenerarImpactoFinanciero, "Alta de contrato", ct);
        return contrato;
    }

    public async Task<Contrato> ActualizarAsync(Commands.UpdateContratoCommand request, CancellationToken ct)
    {
        var contrato = await db.Contratos.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró el contrato ID {request.Id}.");

        await ValidarSolapamientoAsync(request.Id, contrato.TerceroId, contrato.SucursalId, request.FechaInicio, request.FechaFin, ct);

        contrato.Actualizar(request.Descripcion, request.FechaInicio, request.FechaFin, request.Importe, request.RenovacionAutomatica, request.Observacion, currentUser.UserId);
        contratoRepo.Update(contrato);
        await historialRepo.AddAsync(ContratoHistorial.Registrar(contrato.Id, TipoEventoContrato.Actualizacion, request.FechaInicio, "Actualización de contrato.", contrato.Importe, currentUser.UserId), ct);
        await RegistrarImpactosAsync(contrato, request.FechaInicio, request.GenerarImpactoComercial, request.GenerarImpactoFinanciero, "Actualización de contrato", ct);
        return contrato;
    }

    public async Task<Contrato> RenovarAsync(Commands.RenovarContratoCommand request, CancellationToken ct)
    {
        var contrato = await db.Contratos.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró el contrato ID {request.Id}.");

        var nuevaFechaInicio = contrato.FechaFin.AddDays(1);
        await ValidarSolapamientoAsync(request.Id, contrato.TerceroId, contrato.SucursalId, nuevaFechaInicio, request.NuevaFechaFin, ct);

        contrato.Renovar(request.NuevaFechaFin, request.NuevoImporte, request.Observacion, currentUser.UserId);
        contratoRepo.Update(contrato);
        await historialRepo.AddAsync(ContratoHistorial.Registrar(contrato.Id, TipoEventoContrato.Renovacion, contrato.FechaInicio, "Renovación contractual.", contrato.Importe, currentUser.UserId), ct);
        await RegistrarImpactosAsync(contrato, contrato.FechaInicio, request.GenerarImpactoComercial, request.GenerarImpactoFinanciero, "Renovación de contrato", ct);
        return contrato;
    }

    public async Task CancelarAsync(Commands.CancelarContratoCommand request, CancellationToken ct)
    {
        var contrato = await db.Contratos.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró el contrato ID {request.Id}.");

        contrato.Cancelar(request.Observacion, currentUser.UserId);
        contratoRepo.Update(contrato);
        await historialRepo.AddAsync(ContratoHistorial.Registrar(contrato.Id, TipoEventoContrato.Cancelacion, DateOnly.FromDateTime(DateTime.Today), "Cancelación de contrato.", null, currentUser.UserId), ct);
    }

    public async Task<ContratoImpacto> RegistrarImpactoManualAsync(RegistrarImpactoContratoCommand request, CancellationToken ct)
    {
        var contrato = await db.Contratos.FirstOrDefaultAsync(x => x.Id == request.ContratoId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException($"No se encontró el contrato ID {request.ContratoId}.");

        if (contrato.Estado == EstadoContrato.Cancelado)
            throw new InvalidOperationException("No se puede impactar un contrato cancelado.");

        if (request.ImpactarCuentaCorriente && request.Tipo == TipoImpactoContrato.Financiero && request.Importe > 0)
            await cuentaCorrienteService.DebitarAsync(contrato.TerceroId, contrato.SucursalId, contrato.MonedaId, request.Importe, null, request.Fecha, $"Impacto manual contrato #{contrato.Codigo}", ct);

        var impacto = ContratoImpacto.Registrar(contrato.Id, request.Tipo, request.Fecha, request.Importe, request.Descripcion, currentUser.UserId);
        await impactoRepo.AddAsync(impacto, ct);
        await historialRepo.AddAsync(ContratoHistorial.Registrar(
            contrato.Id,
            request.Tipo == TipoImpactoContrato.Comercial ? TipoEventoContrato.ImpactoComercial : TipoEventoContrato.ImpactoFinanciero,
            request.Fecha,
            request.Descripcion,
            request.Importe,
            currentUser.UserId), ct);
        return impacto;
    }

    public async Task<int> FinalizarVencidosAsync(long? sucursalId, DateOnly fechaCorte, CancellationToken ct)
    {
        var query = db.Contratos.Where(x => !x.IsDeleted
            && x.Estado != EstadoContrato.Cancelado
            && x.Estado != EstadoContrato.Finalizado
            && x.FechaFin < fechaCorte);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        var contratos = await query.ToListAsync(ct);
        foreach (var contrato in contratos)
        {
            contrato.Finalizar($"Finalizado por vencimiento al {fechaCorte:yyyy-MM-dd}.", currentUser.UserId);
            contratoRepo.Update(contrato);
            await historialRepo.AddAsync(ContratoHistorial.Registrar(contrato.Id, TipoEventoContrato.Finalizacion, fechaCorte, "Finalización por vencimiento.", null, currentUser.UserId), ct);
        }

        return contratos.Count;
    }

    public async Task<int> RenovarAutomaticamenteAsync(long? sucursalId, DateOnly fechaCorte, decimal? porcentajeAjuste, bool generarImpactoComercial, bool generarImpactoFinanciero, CancellationToken ct)
    {
        var query = db.Contratos.Where(x => !x.IsDeleted
            && x.RenovacionAutomatica
            && x.Estado != EstadoContrato.Cancelado
            && x.Estado != EstadoContrato.Finalizado
            && x.FechaFin <= fechaCorte);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        var contratos = await query.OrderBy(x => x.FechaFin).ThenBy(x => x.Id).ToListAsync(ct);
        var renovados = 0;

        foreach (var contrato in contratos)
        {
            var duracionDias = contrato.FechaFin.DayNumber - contrato.FechaInicio.DayNumber + 1;
            var nuevaFechaFin = contrato.FechaFin.AddDays(duracionDias);
            var nuevoImporte = porcentajeAjuste.HasValue
                ? decimal.Round(contrato.Importe * (1 + (porcentajeAjuste.Value / 100m)), 4, MidpointRounding.AwayFromZero)
                : contrato.Importe;

            await ValidarSolapamientoAsync(contrato.Id, contrato.TerceroId, contrato.SucursalId, contrato.FechaFin.AddDays(1), nuevaFechaFin, ct);

            contrato.Renovar(nuevaFechaFin, nuevoImporte, $"Renovación automática al {fechaCorte:yyyy-MM-dd}.", currentUser.UserId);
            contratoRepo.Update(contrato);
            await historialRepo.AddAsync(ContratoHistorial.Registrar(contrato.Id, TipoEventoContrato.Renovacion, contrato.FechaInicio, "Renovación contractual automática.", contrato.Importe, currentUser.UserId), ct);
            await RegistrarImpactosAsync(contrato, contrato.FechaInicio, generarImpactoComercial, generarImpactoFinanciero, "Renovación automática de contrato", ct);
            renovados++;
        }

        return renovados;
    }

    public async Task<ReporteTabularDto> GetReporteVencimientosAsync(long? sucursalId, int dias, CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var hasta = hoy.AddDays(dias);
        var query = db.Contratos.AsNoTracking()
            .Where(x => !x.IsDeleted && x.Estado != EstadoContrato.Cancelado && x.Estado != EstadoContrato.Finalizado && x.FechaFin >= hoy && x.FechaFin <= hasta);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        var items = await query.OrderBy(x => x.FechaFin).ThenBy(x => x.Id).ToListAsync(ct);
        return new ReporteTabularDto
        {
            Titulo = "Vencimientos de Contratos",
            Parametros = new Dictionary<string, string>
            {
                ["SucursalId"] = sucursalId?.ToString() ?? "Todas",
                ["Dias"] = dias.ToString()
            },
            Columnas = ["Id", "Codigo", "TerceroId", "SucursalId", "FechaFin", "Importe", "RenovacionAutomatica", "Estado"],
            Filas = items.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Id.ToString(),
                x.Codigo,
                x.TerceroId.ToString(),
                x.SucursalId.ToString(),
                x.FechaFin.ToString("yyyy-MM-dd"),
                x.Importe.ToString("0.00"),
                x.RenovacionAutomatica ? "SI" : "NO",
                x.Estado.ToString().ToUpperInvariant()
            }).ToList().AsReadOnly()
        };
    }

    public async Task<ReporteTabularDto> GetReporteHistorialAsync(long? contratoId, long? terceroId, long? sucursalId, TipoEventoContrato? tipoEvento, DateOnly? desde, DateOnly? hasta, CancellationToken ct)
    {
        var contratos = db.Contratos.AsNoTracking().Where(x => !x.IsDeleted);
        if (contratoId.HasValue)
            contratos = contratos.Where(x => x.Id == contratoId.Value);
        if (terceroId.HasValue)
            contratos = contratos.Where(x => x.TerceroId == terceroId.Value);
        if (sucursalId.HasValue)
            contratos = contratos.Where(x => x.SucursalId == sucursalId.Value);

        var contratoItems = await contratos.Select(x => new { x.Id, x.Codigo, x.TerceroId, x.SucursalId }).ToListAsync(ct);
        var contratoIds = contratoItems.Select(x => x.Id).ToList();

        var historialQuery = db.ContratosHistorial.AsNoTracking().Where(x => !x.IsDeleted && contratoIds.Contains(x.ContratoId));
        if (tipoEvento.HasValue)
            historialQuery = historialQuery.Where(x => x.TipoEvento == tipoEvento.Value);
        if (desde.HasValue)
            historialQuery = historialQuery.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue)
            historialQuery = historialQuery.Where(x => x.Fecha <= hasta.Value);

        var historial = await historialQuery.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        var contratosLookup = contratoItems.ToDictionary(x => x.Id);

        return new ReporteTabularDto
        {
            Titulo = "Historial de Contratos",
            Parametros = new Dictionary<string, string>
            {
                ["ContratoId"] = contratoId?.ToString() ?? "Todos",
                ["TerceroId"] = terceroId?.ToString() ?? "Todos",
                ["SucursalId"] = sucursalId?.ToString() ?? "Todas",
                ["TipoEvento"] = tipoEvento?.ToString().ToUpperInvariant() ?? "Todos",
                ["Desde"] = desde?.ToString("yyyy-MM-dd") ?? "—",
                ["Hasta"] = hasta?.ToString("yyyy-MM-dd") ?? "—"
            },
            Columnas = ["Id", "ContratoId", "Codigo", "TerceroId", "SucursalId", "Fecha", "TipoEvento", "Importe", "Descripcion"],
            Filas = historial.Select(x =>
            {
                var contrato = contratosLookup.GetValueOrDefault(x.ContratoId);
                return (IReadOnlyList<string>)new[]
                {
                    x.Id.ToString(),
                    x.ContratoId.ToString(),
                    contrato?.Codigo ?? "—",
                    contrato?.TerceroId.ToString() ?? "—",
                    contrato?.SucursalId.ToString() ?? "—",
                    x.Fecha.ToString("yyyy-MM-dd"),
                    x.TipoEvento.ToString().ToUpperInvariant(),
                    x.Importe?.ToString("0.00") ?? "—",
                    x.Descripcion
                };
            }).ToList().AsReadOnly()
        };
    }

    public async Task<ReporteTabularDto> GetReporteImpactosAsync(long? contratoId, long? terceroId, long? sucursalId, TipoImpactoContrato? tipoImpacto, DateOnly? desde, DateOnly? hasta, CancellationToken ct)
    {
        var contratos = db.Contratos.AsNoTracking().Where(x => !x.IsDeleted);
        if (contratoId.HasValue)
            contratos = contratos.Where(x => x.Id == contratoId.Value);
        if (terceroId.HasValue)
            contratos = contratos.Where(x => x.TerceroId == terceroId.Value);
        if (sucursalId.HasValue)
            contratos = contratos.Where(x => x.SucursalId == sucursalId.Value);

        var contratoItems = await contratos.Select(x => new { x.Id, x.Codigo, x.TerceroId, x.SucursalId }).ToListAsync(ct);
        var contratoIds = contratoItems.Select(x => x.Id).ToList();

        var impactosQuery = db.ContratosImpactos.AsNoTracking().Where(x => !x.IsDeleted && contratoIds.Contains(x.ContratoId));
        if (tipoImpacto.HasValue)
            impactosQuery = impactosQuery.Where(x => x.Tipo == tipoImpacto.Value);
        if (desde.HasValue)
            impactosQuery = impactosQuery.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue)
            impactosQuery = impactosQuery.Where(x => x.Fecha <= hasta.Value);

        var impactos = await impactosQuery.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        var contratosLookup = contratoItems.ToDictionary(x => x.Id);

        return new ReporteTabularDto
        {
            Titulo = "Impactos de Contratos",
            Parametros = new Dictionary<string, string>
            {
                ["ContratoId"] = contratoId?.ToString() ?? "Todos",
                ["TerceroId"] = terceroId?.ToString() ?? "Todos",
                ["SucursalId"] = sucursalId?.ToString() ?? "Todas",
                ["TipoImpacto"] = tipoImpacto?.ToString().ToUpperInvariant() ?? "Todos",
                ["Desde"] = desde?.ToString("yyyy-MM-dd") ?? "—",
                ["Hasta"] = hasta?.ToString("yyyy-MM-dd") ?? "—"
            },
            Columnas = ["Id", "ContratoId", "Codigo", "TerceroId", "SucursalId", "Fecha", "TipoImpacto", "Importe", "Descripcion"],
            Filas = impactos.Select(x =>
            {
                var contrato = contratosLookup.GetValueOrDefault(x.ContratoId);
                return (IReadOnlyList<string>)new[]
                {
                    x.Id.ToString(),
                    x.ContratoId.ToString(),
                    contrato?.Codigo ?? "—",
                    contrato?.TerceroId.ToString() ?? "—",
                    contrato?.SucursalId.ToString() ?? "—",
                    x.Fecha.ToString("yyyy-MM-dd"),
                    x.Tipo.ToString().ToUpperInvariant(),
                    x.Importe.ToString("0.00"),
                    x.Descripcion
                };
            }).ToList().AsReadOnly()
        };
    }

    public async Task<ReporteTabularDto> GetReporteContratosAsync(long? terceroId, long? sucursalId, EstadoContrato? estado, bool soloVigentes, CancellationToken ct)
    {
        var query = db.Contratos.AsNoTracking().Where(x => !x.IsDeleted);
        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);
        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (estado.HasValue)
            query = query.Where(x => x.Estado == estado.Value);
        if (soloVigentes)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            query = query.Where(x => x.FechaInicio <= hoy && x.FechaFin >= hoy && x.Estado != EstadoContrato.Cancelado && x.Estado != EstadoContrato.Finalizado);
        }

        var items = await query.OrderByDescending(x => x.FechaInicio).ThenByDescending(x => x.Id).ToListAsync(ct);
        return new ReporteTabularDto
        {
            Titulo = "Contratos",
            Parametros = new Dictionary<string, string>
            {
                ["TerceroId"] = terceroId?.ToString() ?? "Todos",
                ["SucursalId"] = sucursalId?.ToString() ?? "Todas",
                ["Estado"] = estado?.ToString().ToUpperInvariant() ?? "Todos",
                ["SoloVigentes"] = soloVigentes ? "SI" : "NO"
            },
            Columnas = ["Id", "Codigo", "TerceroId", "SucursalId", "Inicio", "Fin", "Importe", "RenovacionAutomatica", "Estado"],
            Filas = items.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Id.ToString(),
                x.Codigo,
                x.TerceroId.ToString(),
                x.SucursalId.ToString(),
                x.FechaInicio.ToString("yyyy-MM-dd"),
                x.FechaFin.ToString("yyyy-MM-dd"),
                x.Importe.ToString("0.00"),
                x.RenovacionAutomatica ? "SI" : "NO",
                x.Estado.ToString().ToUpperInvariant()
            }).ToList().AsReadOnly()
        };
    }

    private async Task ValidarReferenciasAsync(long terceroId, long sucursalId, long monedaId, CancellationToken ct)
    {
        if (!await db.Terceros.AsNoTracking().AnyAsync(x => x.Id == terceroId && !x.IsDeleted, ct))
            throw new InvalidOperationException($"No se encontró el tercero ID {terceroId}.");
        if (!await db.Sucursales.AsNoTracking().AnyAsync(x => x.Id == sucursalId, ct))
            throw new InvalidOperationException($"No se encontró la sucursal ID {sucursalId}.");
        if (!await db.Monedas.AsNoTracking().AnyAsync(x => x.Id == monedaId, ct))
            throw new InvalidOperationException($"No se encontró la moneda ID {monedaId}.");
    }

    private async Task ValidarSolapamientoAsync(long? contratoId, long terceroId, long sucursalId, DateOnly fechaInicio, DateOnly fechaFin, CancellationToken ct)
    {
        var existe = await db.Contratos.AsNoTracking().AnyAsync(x => !x.IsDeleted
            && x.Id != contratoId
            && x.TerceroId == terceroId
            && x.SucursalId == sucursalId
            && x.Estado != EstadoContrato.Cancelado
            && x.Estado != EstadoContrato.Finalizado
            && fechaInicio <= x.FechaFin
            && fechaFin >= x.FechaInicio, ct);

        if (existe)
            throw new InvalidOperationException("Existe un contrato solapado para el mismo tercero y sucursal en el período indicado.");
    }

    private async Task RegistrarImpactosAsync(Contrato contrato, DateOnly fecha, bool generarImpactoComercial, bool generarImpactoFinanciero, string descripcionBase, CancellationToken ct)
    {
        if (generarImpactoComercial)
        {
            await impactoRepo.AddAsync(ContratoImpacto.Registrar(contrato.Id, TipoImpactoContrato.Comercial, fecha, contrato.Importe, $"{descripcionBase} - impacto comercial.", currentUser.UserId), ct);
            await historialRepo.AddAsync(ContratoHistorial.Registrar(contrato.Id, TipoEventoContrato.ImpactoComercial, fecha, $"{descripcionBase} - impacto comercial.", contrato.Importe, currentUser.UserId), ct);
        }

        if (generarImpactoFinanciero && contrato.Importe > 0)
        {
            await cuentaCorrienteService.DebitarAsync(contrato.TerceroId, contrato.SucursalId, contrato.MonedaId, contrato.Importe, null, fecha, $"{descripcionBase} #{contrato.Codigo}", ct);
            await impactoRepo.AddAsync(ContratoImpacto.Registrar(contrato.Id, TipoImpactoContrato.Financiero, fecha, contrato.Importe, $"{descripcionBase} - impacto financiero.", currentUser.UserId), ct);
            await historialRepo.AddAsync(ContratoHistorial.Registrar(contrato.Id, TipoEventoContrato.ImpactoFinanciero, fecha, $"{descripcionBase} - impacto financiero.", contrato.Importe, currentUser.UserId), ct);
        }
    }
}
