using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Compras.Common;
using ZuluIA_Back.Application.Features.Compras.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class RegistrarRecepcionOrdenCompraCommandHandler(
    IRepository<OrdenCompraMeta> ordenCompraRepo,
    IComprobanteRepository comprobanteRepo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    CircuitoComprasService circuitoCompras)
    : IRequestHandler<RegistrarRecepcionOrdenCompraCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarRecepcionOrdenCompraCommand request, CancellationToken ct)
    {
        var orden = await ordenCompraRepo.GetByIdAsync(request.OrdenCompraId, ct);
        if (orden is null)
            return Result.Failure<long>($"No se encontró la orden de compra ID {request.OrdenCompraId}.");

        var comprobanteOrden = await comprobanteRepo.GetByIdConItemsAsync(orden.ComprobanteId, ct);
        if (comprobanteOrden is null)
            return Result.Failure<long>("No se encontró el comprobante asociado a la orden de compra.");

        orden.RegistrarRecepcion(request.CantidadRecibida, request.FechaRecepcion);
        ordenCompraRepo.Update(orden);

        Comprobante? remitoCreado = null;
        if (request.TipoComprobanteRemitoId.HasValue)
        {
            var tipo = await db.TiposComprobante
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.TipoComprobanteRemitoId.Value, ct);

            if (tipo is null)
                return Result.Failure<long>($"No se encontró el tipo de comprobante ID {request.TipoComprobanteRemitoId.Value}.");

            short prefijo = 0;
            long numero = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (comprobanteOrden.PuntoFacturacionId.HasValue)
            {
                var punto = await db.PuntosFacturacion
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == comprobanteOrden.PuntoFacturacionId.Value, ct);

                if (punto is null)
                    return Result.Failure<long>($"No se encontró el punto de facturación ID {comprobanteOrden.PuntoFacturacionId.Value}.");

                prefijo = punto.Numero;
                numero = await comprobanteRepo.GetProximoNumeroAsync(
                    comprobanteOrden.PuntoFacturacionId.Value,
                    request.TipoComprobanteRemitoId.Value,
                    ct);
            }

            var remito = Comprobante.Crear(
                comprobanteOrden.SucursalId,
                comprobanteOrden.PuntoFacturacionId,
                request.TipoComprobanteRemitoId.Value,
                prefijo,
                numero,
                request.FechaRecepcion,
                null,
                comprobanteOrden.TerceroId,
                comprobanteOrden.MonedaId,
                comprobanteOrden.Cotizacion,
                request.Observacion,
                currentUser.UserId);

            remito.VincularAComprobanteOrigen(comprobanteOrden.Id, currentUser.UserId);

            var cantidadTotalOrden = comprobanteOrden.Items.Sum(x => x.Cantidad - x.CantidadBonificada);
            short ordenItem = 0;
            foreach (var item in comprobanteOrden.Items.OrderBy(x => x.Orden))
            {
                var proporcion = cantidadTotalOrden == 0
                    ? 0m
                    : (item.Cantidad - item.CantidadBonificada) / cantidadTotalOrden;
                var cantidadRecibida = Math.Round(request.CantidadRecibida * proporcion, 4);
                if (cantidadRecibida <= 0)
                    continue;

                var linea = ComprobanteItem.Crear(
                    0,
                    item.ItemId,
                    item.Descripcion,
                    cantidadRecibida,
                    0,
                    item.PrecioUnitario,
                    item.DescuentoPct,
                    item.AlicuotaIvaId,
                    item.PorcentajeIva,
                    item.DepositoId,
                    ordenItem++,
                    item.EsGravado);

                remito.AgregarItem(linea);
            }

            remito.Emitir(currentUser.UserId);
            await comprobanteRepo.AddAsync(remito, ct);
            remitoCreado = remito;

            await circuitoCompras.AplicarEfectosAsync(
                remito,
                tipo,
                OperacionStockCompra.Ingreso,
                request.RemitoValorizado ? OperacionCuentaCorrienteCompra.Debito : OperacionCuentaCorrienteCompra.Ninguna,
                ct);
        }

        await uow.SaveChangesAsync(ct);
        return Result.Success(remitoCreado?.Id ?? orden.Id);
    }
}
