using MediatR;
using ZuluIA_Back.Application.Features.RRHH.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class UpdateLiquidacionSueldoCommandHandler(RrhhService service, IUnitOfWork uow) : IRequestHandler<UpdateLiquidacionSueldoCommand, Result>
{
    public async Task<Result> Handle(UpdateLiquidacionSueldoCommand request, CancellationToken ct)
    {
        try
        {
            await service.ActualizarLiquidacionAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
