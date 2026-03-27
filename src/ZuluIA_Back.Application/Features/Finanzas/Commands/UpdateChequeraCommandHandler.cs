using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class UpdateChequeraCommandHandler(
    IRepository<Chequera> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateChequeraCommand, Result>
{
    public async Task<Result> Handle(UpdateChequeraCommand request, CancellationToken ct)
    {
        var chequera = await repo.GetByIdAsync(request.Id, ct);
        if (chequera is null)
            return Result.Failure($"Chequera {request.Id} no encontrada.");

        chequera.Actualizar(request.Banco, request.NroCuenta, request.Observacion, currentUser.UserId);
        repo.Update(chequera);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}