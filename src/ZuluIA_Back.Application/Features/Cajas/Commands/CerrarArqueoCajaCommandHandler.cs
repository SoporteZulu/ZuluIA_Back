using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public class CerrarArqueoCajaCommandHandler(
    ICajaRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CerrarArqueoCajaCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CerrarArqueoCajaCommand request, CancellationToken ct)
    {
        var caja = await repo.GetByIdAsync(request.Id, ct);

        if (caja is null)
            return Result.Failure<int>($"No se encontro la caja con ID {request.Id}.");

        var nroCierre = caja.CerrarArqueo(currentUser.UserId);
        repo.Update(caja);
        await uow.SaveChangesAsync(ct);

        return Result.Success(nroCierre);
    }
}