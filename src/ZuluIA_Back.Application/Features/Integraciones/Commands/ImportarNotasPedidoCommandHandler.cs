using MediatR;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Application.Features.Ventas.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class ImportarNotasPedidoCommandHandler(
    IMediator mediator,
    IUnitOfWork uow,
    IntegracionProcesoService procesoService)
    : IRequestHandler<ImportarNotasPedidoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(ImportarNotasPedidoCommand request, CancellationToken ct)
    {
        var job = await procesoService.CrearJobAsync(Domain.Enums.TipoProcesoIntegracion.ImportacionNotasPedido, "Importación de notas de pedido", request.NotasPedido.Count, request.Observacion, ct);
        await uow.SaveChangesAsync(ct);

        try
        {
            foreach (var nota in request.NotasPedido)
            {
                var borrador = await mediator.Send(new CrearBorradorVentaCommand(
                    nota.SucursalId,
                    null,
                    nota.TipoComprobanteId,
                    nota.Fecha,
                    nota.FechaVencimiento,
                    nota.TerceroId,
                    nota.MonedaId,
                    nota.Cotizacion,
                    nota.Percepciones,
                    nota.Observacion,
                    null,
                    nota.Items), ct);

                if (!borrador.IsSuccess)
                {
                    procesoService.RegistrarError(job, borrador.Error);
                    await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Error, borrador.Error ?? "Error al importar nota de pedido.", nota.ReferenciaExterna, null, ct);
                    continue;
                }

                procesoService.RegistrarExito(job);
                await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Informacion, "Nota de pedido importada correctamente.", nota.ReferenciaExterna, borrador.Value.ToString(), ct);
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
