using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class ActualizarCumplimientoPedidoCommandHandler(
    IComprobanteRepository comprobanteRepository,
    IApplicationDbContext db,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<ActualizarCumplimientoPedidoCommand, Result>
{
    public async Task<Result> Handle(ActualizarCumplimientoPedidoCommand request, CancellationToken ct)
    {
        var pedido = await comprobanteRepository.GetByIdConItemsAsync(request.PedidoId, ct);
        if (pedido is null)
            return Result.Failure($"No se encontró el pedido ID {request.PedidoId}.");

        var tipoPedido = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => x.Id == pedido.TipoComprobanteId)
            .Select(x => new { x.Codigo, x.Descripcion })
            .FirstOrDefaultAsync(ct);

        if (tipoPedido is null || !PedidoWorkflowRules.EsPedido(tipoPedido.Codigo, tipoPedido.Descripcion))
            return Result.Failure("El comprobante indicado no corresponde a un pedido de ventas.");

        if (!pedido.EstadoPedido.HasValue)
            pedido.InicializarComoPedido(pedido.FechaEntregaCompromiso, currentUser.UserId);

        var documentosHijos = await db.Comprobantes
            .Include(x => x.Items)
            .Where(x => x.ComprobanteOrigenId == pedido.Id && x.Estado != EstadoComprobante.Anulado)
            .ToListAsync(ct);

        var tiposHijos = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => documentosHijos.Select(d => d.TipoComprobanteId).Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.AfectaStock })
            .ToDictionaryAsync(x => x.Id, ct);

        var documentosCumplimiento = documentosHijos
            .Where(x => x.Estado != EstadoComprobante.Borrador)
            .Where(x => tiposHijos.TryGetValue(x.TipoComprobanteId, out var tipo)
                && PedidoWorkflowRules.EsRemito(tipo.Codigo, tipo.Descripcion, tipo.AfectaStock))
            .ToList();

        var itemsCumplimiento = documentosCumplimiento
            .SelectMany(documento => documento.Items.Select(item => new ItemEntregaProjection(
                documento.Id,
                documento.Numero.Formateado,
                documento.Fecha,
                item.Id,
                item.ItemId,
                item.ComprobanteItemOrigenId,
                item.Cantidad - item.CantidadBonificada)))
            .Where(x => x.CantidadEntregada > 0)
            .ToList();

        var hayReferenciasExplicitas = itemsCumplimiento.Any(x => x.ComprobanteItemOrigenId.HasValue);

        foreach (var itemPedido in pedido.Items)
        {
            var cantidadEntregada = itemsCumplimiento
                .Where(x => x.ComprobanteItemOrigenId == itemPedido.Id)
                .Sum(x => x.CantidadEntregada);

            if (cantidadEntregada == 0 && !hayReferenciasExplicitas)
            {
                cantidadEntregada = itemsCumplimiento
                    .Where(x => x.ItemId == itemPedido.ItemId)
                    .Sum(x => x.CantidadEntregada);
            }

            itemPedido.ActualizarCumplimiento(cantidadEntregada, pedido.FechaEntregaCompromiso);
        }

        pedido.ActualizarCumplimientoPedido(currentUser.UserId);
        comprobanteRepository.Update(pedido);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    private sealed record ItemEntregaProjection(
        long ComprobanteId,
        string Numero,
        DateOnly Fecha,
        long ComprobanteItemId,
        long ItemId,
        long? ComprobanteItemOrigenId,
        decimal CantidadEntregada);
}
