using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class FacturarDocumentosMasivoCommandHandler(
    FacturacionBatchService service,
    IUnitOfWork uow)
    : IRequestHandler<FacturarDocumentosMasivoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(FacturarDocumentosMasivoCommand request, CancellationToken ct)
    {
        var job = await service.FacturarMasivoAsync(
            request.ComprobanteOrigenIds,
            request.TipoComprobanteDestinoId,
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
