using MediatR;
using ZuluIA_Back.Application.Features.PuntoVenta.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class DesactivarTimbradoFiscalCommandHandler(PuntoVentaFiscalService service, IUnitOfWork uow) : IRequestHandler<DesactivarTimbradoFiscalCommand, Result>
{
    public async Task<Result> Handle(DesactivarTimbradoFiscalCommand request, CancellationToken ct)
    {
        try
        {
            await service.DesactivarTimbradoAsync(request.Id, request.Observacion, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
