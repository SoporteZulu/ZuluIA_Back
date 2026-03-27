using MediatR;
using ZuluIA_Back.Application.Features.Fiscal.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class CerrarPeriodoContableCommandHandler(
    FiscalContabilidadLocalService service,
    IUnitOfWork uow)
    : IRequestHandler<CerrarPeriodoContableCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CerrarPeriodoContableCommand request, CancellationToken ct)
    {
        try
        {
            var cierre = await service.CerrarPeriodoContableAsync(request.EjercicioId, request.SucursalId, request.Desde, request.Hasta, request.Observacion, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(cierre.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
