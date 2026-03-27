using MediatR;
using ZuluIA_Back.Application.Features.RRHH.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class ImputarLiquidacionEmpleadoCommandHandler(RrhhService service, IUnitOfWork uow) : IRequestHandler<ImputarLiquidacionEmpleadoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(ImputarLiquidacionEmpleadoCommand request, CancellationToken ct)
    {
        try
        {
            var imputacion = await service.ImputarLiquidacionAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(imputacion.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
