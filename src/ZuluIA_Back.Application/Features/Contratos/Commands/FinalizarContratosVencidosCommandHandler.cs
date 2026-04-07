using MediatR;
using ZuluIA_Back.Application.Features.Contratos.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public class FinalizarContratosVencidosCommandHandler(ContratosService service, IUnitOfWork uow) : IRequestHandler<FinalizarContratosVencidosCommand, Result<int>>
{
    public async Task<Result<int>> Handle(FinalizarContratosVencidosCommand request, CancellationToken ct)
    {
        try
        {
            var total = await service.FinalizarVencidosAsync(request.SucursalId, request.FechaCorte, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(total);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<int>(ex.Message);
        }
    }
}
