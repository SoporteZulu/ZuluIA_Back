using MediatR;
using ZuluIA_Back.Application.Features.RRHH.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class CambiarEstadoEmpleadoCommandHandler(RrhhService service, IUnitOfWork uow) : IRequestHandler<CambiarEstadoEmpleadoCommand, Result>
{
    public async Task<Result> Handle(CambiarEstadoEmpleadoCommand request, CancellationToken ct)
    {
        try
        {
            await service.CambiarEstadoEmpleadoAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
