using MediatR;
using ZuluIA_Back.Application.Features.PuntoVenta.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class RegistrarComprobantePosCommandHandler(PuntoVentaFiscalService service, IUnitOfWork uow)
    : IRequestHandler<RegistrarComprobantePosCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarComprobantePosCommand request, CancellationToken ct)
    {
        try
        {
            var mapped = new RegistrarComprobantePuntoVentaCommand(
                request.SucursalId,
                request.PuntoFacturacionId,
                request.TipoComprobanteId,
                request.Fecha,
                request.FechaVencimiento,
                request.TerceroId,
                request.MonedaId,
                request.Cotizacion,
                request.Percepciones,
                request.Observacion,
                request.AfectaStock,
                request.ReferenciaExterna,
                request.Items);

            var id = await service.RegistrarComprobanteAsync(mapped, CanalOperacionPuntoVenta.Pos, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
