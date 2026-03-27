using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Application.Features.Ventas.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class ImportarVentasCommandHandler(
    IMediator mediator,
    IUnitOfWork uow,
    IntegracionProcesoService procesoService)
    : IRequestHandler<ImportarVentasCommand, Result<long>>
{
    public async Task<Result<long>> Handle(ImportarVentasCommand request, CancellationToken ct)
    {
        var job = await procesoService.CrearJobAsync(Domain.Enums.TipoProcesoIntegracion.ImportacionVentas, "Importación de ventas", request.Ventas.Count, request.Observacion, ct);
        await uow.SaveChangesAsync(ct);

        try
        {
            foreach (var venta in request.Ventas)
            {
                var borrador = await mediator.Send(new CrearBorradorVentaCommand(
                    venta.SucursalId,
                    venta.PuntoFacturacionId,
                    venta.TipoComprobanteId,
                    venta.Fecha,
                    venta.FechaVencimiento,
                    venta.TerceroId,
                    venta.MonedaId,
                    venta.Cotizacion,
                    venta.Percepciones,
                    venta.Observacion,
                    venta.ComprobanteOrigenId,
                    venta.Items), ct);

                if (!borrador.IsSuccess)
                {
                    procesoService.RegistrarError(job, borrador.Error);
                    await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Error, borrador.Error ?? "Error al crear venta importada.", venta.ReferenciaExterna, null, ct);
                    continue;
                }

                if (venta.Emitir)
                {
                    var emitido = await mediator.Send(new EmitirDocumentoVentaCommand(
                        borrador.Value,
                        venta.OperacionStock,
                        venta.OperacionCuentaCorriente), ct);

                    if (!emitido.IsSuccess)
                    {
                        procesoService.RegistrarError(job, emitido.Error);
                        await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Error, emitido.Error ?? "Error al emitir venta importada.", venta.ReferenciaExterna, borrador.Value.ToString(), ct);
                        continue;
                    }
                }

                procesoService.RegistrarExito(job);
                await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Informacion, "Venta importada correctamente.", venta.ReferenciaExterna, borrador.Value.ToString(), ct);
            }

            procesoService.Finalizar(job, request.Observacion);
            await uow.SaveChangesAsync(ct);
            return Result.Success(job.Id);
        }
        catch (Exception ex)
        {
            procesoService.Fallar(job, ex.Message);
            await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Error, ex.Message, null, null, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Failure<long>(ex.Message);
        }
    }
}
