using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class ConfirmarCartaPorteCommandHandler(
    ICartaPorteRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<ConfirmarCartaPorteCommand, Result>
{
    public async Task<Result> Handle(ConfirmarCartaPorteCommand request, CancellationToken ct)
    {
        var carta = await repo.GetByIdAsync(request.CartaPorteId, ct);
        if (carta is null)
            return Result.Failure($"No se encontro la carta de porte con ID {request.CartaPorteId}.");

        carta.Confirmar(currentUser.UserId);
        repo.Update(carta);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}