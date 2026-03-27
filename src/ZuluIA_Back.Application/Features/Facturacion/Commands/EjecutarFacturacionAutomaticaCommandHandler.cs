using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class EjecutarFacturacionAutomaticaCommandHandler(
    FacturacionBatchService service,
    IUnitOfWork uow)
    : IRequestHandler<EjecutarFacturacionAutomaticaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(EjecutarFacturacionAutomaticaCommand request, CancellationToken ct)
    {
        var job = await service.FacturarAutomaticoAsync(
            request.SucursalId,
            request.TipoComprobanteOrigenId,
            request.TipoComprobanteDestinoId,
            request.Desde,
            request.Hasta,
            request.TerceroId,
            request.SoloEmitidos,
            request.PuntoFacturacionId,
            request.Fecha,
            request.FechaVencimiento,
            request.Observacion,
            request.OperacionStock,
            request.OperacionCuentaCorriente,
            request.AutorizarAfip,
            request.UsarCaea,
            request.ClaveIdempotencia,
            ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success(job.Id);
    }
}
