using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class CerrarPedidoCommandHandler(
    IComprobanteRepository comprobanteRepository,
    IApplicationDbContext db,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<CerrarPedidoCommand, Result>
{
    public async Task<Result> Handle(CerrarPedidoCommand request, CancellationToken ct)
    {
        var pedido = await comprobanteRepository.GetByIdConItemsAsync(request.PedidoId, ct);
        if (pedido is null)
            return Result.Failure($"No se encontró el pedido ID {request.PedidoId}.");

        var tipo = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => x.Id == pedido.TipoComprobanteId)
            .Select(x => new { x.Codigo, x.Descripcion })
            .FirstOrDefaultAsync(ct);

        if (tipo is null || !PedidoWorkflowRules.EsPedido(tipo.Codigo, tipo.Descripcion))
            return Result.Failure("El comprobante indicado no corresponde a un pedido de ventas.");

        if (!pedido.EstadoPedido.HasValue)
            pedido.InicializarComoPedido(pedido.FechaEntregaCompromiso, currentUser.UserId);

        if (pedido.Estado == EstadoComprobante.Anulado)
            return Result.Failure("No se puede cerrar un pedido anulado.");

        try
        {
            pedido.CerrarPedido(request.MotivoCierre, currentUser.UserId);
            comprobanteRepository.Update(pedido);
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
