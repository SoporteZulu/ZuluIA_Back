using MediatR;
using ZuluIA_Back.Application.Features.PuntoVenta.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class RegistrarComprobantePuntoVentaCommandHandler(PuntoVentaFiscalService service, IUnitOfWork uow)
    : IRequestHandler<RegistrarComprobantePuntoVentaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarComprobantePuntoVentaCommand request, CancellationToken ct)
    {
        try
        {
            var id = await service.RegistrarComprobanteAsync(request, CanalOperacionPuntoVenta.PuntoVenta, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
