using MediatR;
using ZuluIA_Back.Application.Features.PuntoVenta.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class ConciliarSifenOperacionCommandHandler(PuntoVentaFiscalService service, IUnitOfWork uow) : IRequestHandler<ConciliarSifenOperacionCommand, Result>
{
    public async Task<Result> Handle(ConciliarSifenOperacionCommand request, CancellationToken ct)
    {
        try
        {
            await service.ConciliarSifenAsync(request.Id, request.Confirmar, request.Observacion, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
