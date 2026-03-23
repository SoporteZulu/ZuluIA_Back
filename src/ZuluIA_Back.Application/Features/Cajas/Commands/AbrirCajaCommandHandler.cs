using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public class AbrirCajaCommandHandler(
    ICajaRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AbrirCajaCommand, Result>
{
    public async Task<Result> Handle(AbrirCajaCommand request, CancellationToken ct)
    {
        var caja = await repo.GetByIdAsync(request.Id, ct);

        if (caja is null)
            return Result.Failure($"No se encontro la caja con ID {request.Id}.");

        caja.AbrirCaja(request.FechaApertura, request.SaldoInicial, currentUser.UserId);
        repo.Update(caja);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}