using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class BatchSchedulerService(
    IApplicationDbContext db,
    IRepository<BatchProgramacion> repo,
    IMediator mediator,
    OperacionesBatchSettingsService settingsService,
    ICurrentUserService currentUser)
{
    public async Task<BatchProgramacion> ProgramarFacturacionAutomaticaAsync(EjecutarFacturacionAutomaticaCommand command, int intervaloMinutos, DateTimeOffset primeraEjecucion, string? observacion, CancellationToken ct)
    {
        var entity = BatchProgramacion.Crear(
            TipoProcesoIntegracion.FacturacionAutomatica,
            "Scheduler facturación automática",
            intervaloMinutos,
            primeraEjecucion,
            JsonSerializer.Serialize(command),
            observacion,
            currentUser.UserId);

        await repo.AddAsync(entity, ct);
        return entity;
    }

    public async Task<int> ProcesarPendientesAsync(CancellationToken ct)
    {
        var settings = await settingsService.ResolveAsync(ct);
        if (!settings.SchedulerHabilitado)
            return 0;

        var ahora = DateTimeOffset.UtcNow;
        var pendientes = await db.BatchProgramaciones
            .Where(x => x.Activa
                && x.TipoProceso == TipoProcesoIntegracion.FacturacionAutomatica
                && x.ProximaEjecucion <= ahora
                && x.DeletedAt == null)
            .OrderBy(x => x.ProximaEjecucion)
            .Take(settings.SchedulerLote)
            .ToListAsync(ct);

        var procesados = 0;

        foreach (var item in pendientes)
        {
            try
            {
                if (item.TipoProceso != TipoProcesoIntegracion.FacturacionAutomatica)
                    throw new InvalidOperationException($"El tipo de proceso '{item.TipoProceso}' no está soportado por el scheduler actual.");

                var command = JsonSerializer.Deserialize<EjecutarFacturacionAutomaticaCommand>(item.PayloadJson)
                    ?? throw new InvalidOperationException($"No se pudo deserializar la programación batch {item.Id}.");

                await mediator.Send(command, ct);
                item.MarcarEjecutada(ahora, currentUser.UserId);
                procesados++;
            }
            catch (Exception ex)
            {
                item.RegistrarError(ahora, settings.SchedulerReintentoErrorMinutos, ex.Message, currentUser.UserId);
            }

            repo.Update(item);
            await db.SaveChangesAsync(ct);
        }

        return procesados;
    }

    public async Task DesactivarAsync(long id, string? observacion, CancellationToken ct)
    {
        var item = await db.BatchProgramaciones.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct)
            ?? throw new InvalidOperationException($"No se encontró la programación batch ID {id}.");

        item.Desactivar(observacion, currentUser.UserId);
        repo.Update(item);
    }

    public async Task ReactivarAsync(long id, DateTimeOffset proximaEjecucion, string? observacion, CancellationToken ct)
    {
        var item = await db.BatchProgramaciones.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct)
            ?? throw new InvalidOperationException($"No se encontró la programación batch ID {id}.");

        item.Reactivar(proximaEjecucion, observacion, currentUser.UserId);
        repo.Update(item);
    }

    public async Task ActualizarProgramacionAsync(long id, int intervaloMinutos, DateTimeOffset proximaEjecucion, string? observacion, CancellationToken ct)
    {
        var item = await db.BatchProgramaciones.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct)
            ?? throw new InvalidOperationException($"No se encontró la programación batch ID {id}.");

        item.ActualizarProgramacion(intervaloMinutos, proximaEjecucion, observacion, currentUser.UserId);
        repo.Update(item);
    }
}
