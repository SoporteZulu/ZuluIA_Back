using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class IntegracionProcesoService(
    IApplicationDbContext db,
    IRepository<ProcesoIntegracionJob> jobRepo,
    IRepository<ProcesoIntegracionLog> logRepo,
    IRepository<MonitorExportacion> monitorRepo,
    ICurrentUserService currentUser)
{
    public async Task<ProcesoIntegracionJob?> ObtenerPorClaveIdempotenciaAsync(TipoProcesoIntegracion tipo, string? claveIdempotencia, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(claveIdempotencia))
            return null;

        var clave = claveIdempotencia.Trim().ToUpperInvariant();
        return await db.ProcesosIntegracionJobs
            .FirstOrDefaultAsync(x => x.Tipo == tipo && x.ClaveIdempotencia == clave, ct);
    }

    public async Task<ProcesoIntegracionJob> CrearJobAsync(TipoProcesoIntegracion tipo, string nombre, int totalRegistros, string? payloadResumen, CancellationToken ct, string? claveIdempotencia = null)
    {
        var job = ProcesoIntegracionJob.Crear(tipo, nombre, totalRegistros, payloadResumen, currentUser.UserId, claveIdempotencia);
        job.Iniciar(currentUser.UserId);
        await jobRepo.AddAsync(job, ct);
        return job;
    }

    public async Task RegistrarLogAsync(long jobId, NivelLogIntegracion nivel, string mensaje, string? referencia, string? payload, CancellationToken ct)
    {
        await logRepo.AddAsync(ProcesoIntegracionLog.Registrar(jobId, nivel, mensaje, referencia, payload, currentUser.UserId), ct);
    }

    public void RegistrarExito(ProcesoIntegracionJob job) => job.RegistrarExito(currentUser.UserId);

    public void RegistrarError(ProcesoIntegracionJob job, string? observacion) => job.RegistrarError(observacion, currentUser.UserId);

    public void Finalizar(ProcesoIntegracionJob job, string? observacion) => job.Finalizar(observacion, currentUser.UserId);

    public void Fallar(ProcesoIntegracionJob job, string observacion) => job.Fallar(observacion, currentUser.UserId);

    public async Task ActualizarMonitorExportacionAsync(string codigo, string descripcion, ProcesoIntegracionJob job, int registrosPendientes, string? mensaje, CancellationToken ct)
    {
        var monitor = await db.MonitoresExportacion.FirstOrDefaultAsync(x => x.Codigo == codigo.Trim().ToUpperInvariant(), ct);
        if (monitor is null)
        {
            monitor = MonitorExportacion.Crear(codigo, descripcion, currentUser.UserId);
            monitor.Actualizar(job.Id, job.Estado, registrosPendientes, mensaje, currentUser.UserId);
            await monitorRepo.AddAsync(monitor, ct);
            return;
        }

        monitor.Actualizar(job.Id, job.Estado, registrosPendientes, mensaje, currentUser.UserId);
        monitorRepo.Update(monitor);
    }
}
