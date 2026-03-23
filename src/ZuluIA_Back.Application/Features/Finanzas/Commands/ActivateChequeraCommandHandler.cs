using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class ActivateChequeraCommandHandler(
    IRepository<Chequera> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<ActivateChequeraCommand, Result>
{
    public async Task<Result> Handle(ActivateChequeraCommand request, CancellationToken ct)
    {
        var chequera = await repo.GetByIdAsync(request.Id, ct);
        if (chequera is null)
            return Result.Failure($"Chequera {request.Id} no encontrada.");

        chequera.Activar(currentUser.UserId);
        repo.Update(chequera);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}