using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class CerrarPeriodoIvaCommandHandler(
    IPeriodoIvaRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<CerrarPeriodoIvaCommand, Result>
{
    public async Task<Result> Handle(CerrarPeriodoIvaCommand request, CancellationToken ct)
    {
        var periodo = await repo.GetPeriodoAsync(request.SucursalId, request.Periodo, ct);

        if (periodo is null)
            return Result.Failure(
                $"No se encontró el período {request.Periodo:yyyy-MM} " +
                $"para la sucursal {request.SucursalId}.");

        periodo.Cerrar();
        repo.Update(periodo);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}