using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class EjecutarSyncIntegracionCommandHandler(
    IApplicationDbContext db,
    IUnitOfWork uow,
    IntegracionProcesoService procesoService)
    : IRequestHandler<EjecutarSyncIntegracionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(EjecutarSyncIntegracionCommand request, CancellationToken ct)
    {
        var existente = await procesoService.ObtenerPorClaveIdempotenciaAsync(request.Tipo, request.ClaveIdempotencia, ct);
        if (existente is not null)
            return Result.Success(existente.Id);

        var job = await procesoService.CrearJobAsync(request.Tipo, $"Sync {request.CodigoMonitor}", 1, request.Observacion, ct, request.ClaveIdempotencia);
        await uow.SaveChangesAsync(ct);

        try
        {
            var pendientes = request.Tipo switch
            {
                Domain.Enums.TipoProcesoIntegracion.ImportacionClientes => await db.Terceros.AsNoTracking().CountAsync(x => x.EsCliente && x.Activo, ct),
                Domain.Enums.TipoProcesoIntegracion.ImportacionVentas => await db.Comprobantes.AsNoTracking().CountAsync(ct),
                Domain.Enums.TipoProcesoIntegracion.ImportacionNotasPedido => await db.Comprobantes.AsNoTracking().CountAsync(ct),
                _ => 0
            };

            procesoService.RegistrarExito(job);
            await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Informacion, "Sync ejecutado correctamente.", request.CodigoMonitor, pendientes.ToString(), ct);
            procesoService.Finalizar(job, request.Observacion);
            await procesoService.ActualizarMonitorExportacionAsync(request.CodigoMonitor, request.DescripcionMonitor, job, pendientes, request.Observacion ?? "Sync ejecutado", ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(job.Id);
        }
        catch (Exception ex)
        {
            procesoService.Fallar(job, ex.Message);
            await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Error, ex.Message, request.CodigoMonitor, null, ct);
            await procesoService.ActualizarMonitorExportacionAsync(request.CodigoMonitor, request.DescripcionMonitor, job, 0, ex.Message, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Failure<long>(ex.Message);
        }
    }
}
