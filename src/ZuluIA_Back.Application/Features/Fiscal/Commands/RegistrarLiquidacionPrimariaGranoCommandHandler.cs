using MediatR;
using ZuluIA_Back.Application.Features.Fiscal.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class RegistrarLiquidacionPrimariaGranoCommandHandler(
    FiscalContabilidadLocalService service,
    IUnitOfWork uow)
    : IRequestHandler<RegistrarLiquidacionPrimariaGranoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarLiquidacionPrimariaGranoCommand request, CancellationToken ct)
    {
        try
        {
            var liquidacion = await service.RegistrarLiquidacionPrimariaGranoAsync(request.SucursalId, request.Fecha, request.NumeroLiquidacion, request.Producto, request.Cantidad, request.PrecioUnitario, request.Observacion, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(liquidacion.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
