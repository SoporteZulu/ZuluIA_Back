using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.NotasPedido.Commands;

public class AnularNotaPedidoCommandHandler(
    INotaPedidoRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AnularNotaPedidoCommand, Result>
{
    public async Task<Result> Handle(AnularNotaPedidoCommand request, CancellationToken ct)
    {
        var np = await repo.GetByIdAsync(request.Id, ct);
        if (np is null) return Result.Failure("Nota de pedido no encontrada.");
        np.Anular(currentUser.UserId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
