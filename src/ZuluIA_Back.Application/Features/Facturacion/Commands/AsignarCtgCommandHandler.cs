using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class AsignarCtgCommandHandler(
    ICartaPorteRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AsignarCtgCommand, Result>
{
    public async Task<Result> Handle(AsignarCtgCommand request, CancellationToken ct)
    {
        var carta = await repo.GetByIdAsync(request.CartaPorteId, ct);
        if (carta is null)
            return Result.Failure($"No se encontró la carta de porte con ID {request.CartaPorteId}.");

        carta.AsignarCtg(request.NroCtg, currentUser.UserId);
        repo.Update(carta);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}