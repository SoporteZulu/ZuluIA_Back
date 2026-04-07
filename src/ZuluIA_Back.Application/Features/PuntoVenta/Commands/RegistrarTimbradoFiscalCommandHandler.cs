using MediatR;
using ZuluIA_Back.Application.Features.PuntoVenta.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class RegistrarTimbradoFiscalCommandHandler(PuntoVentaFiscalService service, IUnitOfWork uow)
    : IRequestHandler<RegistrarTimbradoFiscalCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarTimbradoFiscalCommand request, CancellationToken ct)
    {
        try
        {
            var timbrado = await service.RegistrarTimbradoAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(timbrado.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
