using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class UsarChequeCommandHandler(
    IRepository<Chequera> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UsarChequeCommand, Result<UsarChequeResult>>
{
    public async Task<Result<UsarChequeResult>> Handle(UsarChequeCommand request, CancellationToken ct)
    {
        var chequera = await repo.GetByIdAsync(request.Id, ct);
        if (chequera is null)
            return Result.Failure<UsarChequeResult>($"Chequera {request.Id} no encontrada.");

        try
        {
            chequera.UsarCheque(request.Numero, currentUser.UserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<UsarChequeResult>(ex.Message);
        }

        repo.Update(chequera);
        await uow.SaveChangesAsync(ct);

        return Result.Success(new UsarChequeResult(chequera.Id, chequera.UltimoChequeUsado));
    }
}