using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public class ActivateCajaCommandHandler(
    ICajaRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<ActivateCajaCommand, Result>
{
    public async Task<Result> Handle(ActivateCajaCommand request, CancellationToken ct)
    {
        var caja = await repo.GetByIdAsync(request.Id, ct);
        if (caja is null)
            return Result.Failure($"No se encontró la caja con ID {request.Id}.");

        caja.Activar(currentUser.UserId);
        repo.Update(caja);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}