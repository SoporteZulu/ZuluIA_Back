using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class CerrarPedidosMasivoCommandHandler(
    IApplicationDbContext db,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<CerrarPedidosMasivoCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CerrarPedidosMasivoCommand request, CancellationToken ct)
    {
        var tiposPedidoIds = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => x.EsVenta)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToListAsync(ct);

        var pedidoTypeIds = tiposPedidoIds
            .Where(x => PedidoWorkflowRules.EsPedido(x.Codigo, x.Descripcion))
            .Select(x => x.Id)
            .ToHashSet();

        var query = db.Comprobantes
            .Include(x => x.Items)
            .Where(x => pedidoTypeIds.Contains(x.TipoComprobanteId)
                || x.EstadoPedido.HasValue
                || x.FechaEntregaCompromiso.HasValue);

        if (request.SucursalId.HasValue)
            query = query.Where(x => x.SucursalId == request.SucursalId.Value);

        if (request.TerceroId.HasValue)
            query = query.Where(x => x.TerceroId == request.TerceroId.Value);

        if (request.FechaDesde.HasValue)
            query = query.Where(x => x.Fecha >= request.FechaDesde.Value);

        if (request.FechaHasta.HasValue)
            query = query.Where(x => x.Fecha <= request.FechaHasta.Value);

        if (request.FechaEntregaDesde.HasValue)
            query = query.Where(x => x.FechaEntregaCompromiso >= request.FechaEntregaDesde.Value);

        if (request.FechaEntregaHasta.HasValue)
            query = query.Where(x => x.FechaEntregaCompromiso <= request.FechaEntregaHasta.Value);

        if (request.SoloPendientes)
        {
            query = query.Where(x => !x.EstadoPedido.HasValue
                || x.EstadoPedido == EstadoPedido.Pendiente
                || x.EstadoPedido == EstadoPedido.EnProceso);
        }

        var pedidos = await query.ToListAsync(ct);
        if (pedidos.Count == 0)
            return Result.Success(0);

        var cerrados = 0;
        foreach (var pedido in pedidos)
        {
            if (!pedido.EstadoPedido.HasValue)
                pedido.InicializarComoPedido(pedido.FechaEntregaCompromiso, currentUser.UserId);

            if (pedido.Estado == EstadoComprobante.Anulado)
                continue;

            if (pedido.EstadoPedido is EstadoPedido.Cerrado or EstadoPedido.Completado or EstadoPedido.Anulado)
                continue;

            pedido.CerrarPedido(request.MotivoCierre, currentUser.UserId);
            cerrados++;
        }

        if (cerrados == 0)
            return Result.Success(0);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(cerrados);
    }
}
