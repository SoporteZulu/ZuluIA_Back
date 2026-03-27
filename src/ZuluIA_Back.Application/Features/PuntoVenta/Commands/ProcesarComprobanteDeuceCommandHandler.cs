using MediatR;
using ZuluIA_Back.Application.Features.PuntoVenta.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class ProcesarComprobanteDeuceCommandHandler(PuntoVentaFiscalService service, IUnitOfWork uow)
    : IRequestHandler<ProcesarComprobanteDeuceCommand, Result<long>>
{
    public async Task<Result<long>> Handle(ProcesarComprobanteDeuceCommand request, CancellationToken ct)
    {
        try
        {
            var operacion = await service.ProcesarDeuceAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(operacion.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
