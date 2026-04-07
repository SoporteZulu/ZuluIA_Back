using MediatR;
using ZuluIA_Back.Application.Features.PuntoVenta.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class UpdateTimbradoFiscalCommandHandler(PuntoVentaFiscalService service, IUnitOfWork uow) : IRequestHandler<UpdateTimbradoFiscalCommand, Result>
{
    public async Task<Result> Handle(UpdateTimbradoFiscalCommand request, CancellationToken ct)
    {
        try
        {
            await service.ActualizarTimbradoAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
