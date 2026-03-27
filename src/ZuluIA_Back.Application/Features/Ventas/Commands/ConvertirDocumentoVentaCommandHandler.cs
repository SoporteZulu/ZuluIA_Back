using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Ventas.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class ConvertirDocumentoVentaCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    CircuitoVentasService circuitoVentas)
    : IRequestHandler<ConvertirDocumentoVentaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(ConvertirDocumentoVentaCommand request, CancellationToken ct)
    {
        var origen = await db.Comprobantes
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.ComprobanteOrigenId && !x.IsDeleted, ct);

        if (origen is null)
            return Result.Failure<long>($"No se encontró el comprobante origen ID {request.ComprobanteOrigenId}.");

        if (origen.Estado is EstadoComprobante.Anulado or EstadoComprobante.Convertido)
            return Result.Failure<long>($"El comprobante origen ya fue {origen.Estado.ToString().ToLowerInvariant()}.");

        if (!origen.Items.Any())
            return Result.Failure<long>("El comprobante origen no tiene ítems para convertir.");

        var tipoDestino = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TipoComprobanteDestinoId, ct);

        if (tipoDestino is null)
            return Result.Failure<long>($"No se encontró el tipo de comprobante ID {request.TipoComprobanteDestinoId}.");

        if (!tipoDestino.EsVenta)
            return Result.Failure<long>("El tipo de comprobante destino no pertenece al circuito de ventas.");

        short prefijo = 0;
        long numero = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (request.PuntoFacturacionId.HasValue)
        {
            var punto = await db.PuntosFacturacion
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.PuntoFacturacionId.Value, ct);

            if (punto is null)
                return Result.Failure<long>($"No se encontró el punto de facturación ID {request.PuntoFacturacionId.Value}.");

            prefijo = punto.Numero;
            numero = await comprobanteRepo.GetProximoNumeroAsync(
                request.PuntoFacturacionId.Value,
                request.TipoComprobanteDestinoId,
                ct);
        }

        var nuevo = Comprobante.Crear(
            origen.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteDestinoId,
            prefijo,
            numero,
            request.Fecha,
            request.FechaVencimiento,
            origen.TerceroId,
            origen.MonedaId,
            origen.Cotizacion,
            request.Observacion ?? origen.Observacion,
            currentUser.UserId);

        nuevo.VincularAComprobanteOrigen(origen.Id, currentUser.UserId);

        short orden = 0;
        foreach (var item in origen.Items.OrderBy(x => x.Orden))
        {
            var linea = ComprobanteItem.Crear(
                0,
                item.ItemId,
                item.Descripcion,
                item.Cantidad,
                item.CantidadBonificada,
                item.PrecioUnitario,
                item.DescuentoPct,
                item.AlicuotaIvaId,
                item.PorcentajeIva,
                item.DepositoId,
                orden++,
                item.EsGravado);

            nuevo.AgregarItem(linea);
        }

        if (origen.Percepciones > 0)
            nuevo.SetPercepciones(origen.Percepciones, currentUser.UserId);

        nuevo.Emitir(currentUser.UserId);

        await comprobanteRepo.AddAsync(nuevo, ct);

        origen.MarcarComoConvertido(currentUser.UserId);
        comprobanteRepo.Update(origen);

        await circuitoVentas.AplicarEfectosAsync(
            nuevo,
            tipoDestino,
            request.OperacionStock,
            request.OperacionCuentaCorriente,
            ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success(nuevo.Id);
    }
}
