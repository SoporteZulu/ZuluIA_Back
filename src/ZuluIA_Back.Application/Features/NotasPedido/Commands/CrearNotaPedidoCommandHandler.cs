using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.NotasPedido.Commands;

public class CrearNotaPedidoCommandHandler(
    INotaPedidoRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CrearNotaPedidoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CrearNotaPedidoCommand request, CancellationToken ct)
    {
        if (!request.Items.Any())
            return Result.Failure<long>("La nota de pedido debe tener al menos un ítem.");

        var np = NotaPedido.Crear(
            request.SucursalId, request.TerceroId,
            request.Fecha, request.FechaVencimiento,
            request.Observacion, request.VendedorId,
            currentUser.UserId);

        foreach (var item in request.Items)
            np.AgregarItem(NotaPedidoItem.Crear(
                0, item.ItemId, item.Cantidad,
                item.PrecioUnitario, item.Bonificacion));

        await repo.AddAsync(np, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(np.Id);
    }
}
