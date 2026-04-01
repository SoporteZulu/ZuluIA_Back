using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Services;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class CrearBorradorVentaCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    TerceroOperacionValidationService terceroOperacionValidationService)
    : IRequestHandler<CrearBorradorVentaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CrearBorradorVentaCommand request, CancellationToken ct)
    {
        var tipo = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TipoComprobanteId, ct);

        if (tipo is null)
            return Result.Failure<long>($"No se encontró el tipo de comprobante ID {request.TipoComprobanteId}.");

        if (!tipo.EsVenta)
            return Result.Failure<long>("El tipo de comprobante indicado no pertenece al circuito de ventas.");

        var validationError = await terceroOperacionValidationService.ValidateClienteAsync(request.TerceroId, ct);
        if (validationError is not null)
            return Result.Failure<long>(validationError);

        if (request.ComprobanteOrigenId.HasValue)
        {
            var origen = await comprobanteRepo.GetByIdAsync(request.ComprobanteOrigenId.Value, ct);
            if (origen is null)
                return Result.Failure<long>($"No se encontró el comprobante origen ID {request.ComprobanteOrigenId.Value}.");

            if (origen.TerceroId != request.TerceroId)
                return Result.Failure<long>("El comprobante origen pertenece a otro tercero.");
        }

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
                request.TipoComprobanteId,
                ct);
        }

        var comprobante = Comprobante.Crear(
            request.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            prefijo,
            numero,
            request.Fecha,
            request.FechaVencimiento,
            request.TerceroId,
            request.MonedaId,
            request.Cotizacion,
            request.Observacion,
            currentUser.UserId);

        var zonaComercialId = await db.TercerosPerfilesComerciales
            .AsNoTracking()
            .Where(x => x.TerceroId == request.TerceroId)
            .Select(x => x.ZonaComercialId)
            .FirstOrDefaultAsync(ct);

        comprobante.AsignarDatosComerciales(
            request.VendedorId,
            null,
            zonaComercialId,
            request.ListaPreciosId,
            request.CondicionPagoId,
            request.PlazoDias,
            request.CanalVentaId,
            null,
            null,
            currentUser.UserId);

        var alicuotaIds = request.Items.Select(x => x.AlicuotaIvaId).Distinct().ToList();
        var alicuotas = await db.AlicuotasIva
            .AsNoTracking()
            .Where(x => alicuotaIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        short orden = 0;
        foreach (var itemInput in request.Items.OrderBy(x => x.Orden))
        {
            if (!alicuotas.TryGetValue(itemInput.AlicuotaIvaId, out var alicuota))
                return Result.Failure<long>($"No se encontró la alícuota de IVA ID {itemInput.AlicuotaIvaId}.");

            var itemDb = await db.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == itemInput.ItemId, ct);

            if (itemDb is null)
                return Result.Failure<long>($"No se encontró el ítem ID {itemInput.ItemId}.");

            var linea = ComprobanteItem.Crear(
                0,
                itemInput.ItemId,
                itemInput.Descripcion ?? itemDb.Descripcion,
                itemInput.Cantidad,
                itemInput.CantidadBonificada,
                itemInput.PrecioUnitario,
                itemInput.DescuentoPct,
                itemInput.AlicuotaIvaId,
                alicuota.Porcentaje,
                itemInput.DepositoId,
                orden++,
                true,
                itemInput.Lote,
                itemInput.Serie,
                itemInput.FechaVencimiento,
                itemInput.UnidadMedidaId,
                itemInput.ObservacionRenglon,
                itemInput.PrecioListaOriginal,
                itemInput.ComisionVendedorRenglon,
                itemInput.ComprobanteItemOrigenId);

            if (itemInput.CantidadDocumentoOrigen.HasValue || itemInput.PrecioDocumentoOrigen.HasValue)
            {
                linea.SetDatosDocumentoOrigen(itemInput.CantidadDocumentoOrigen, itemInput.PrecioDocumentoOrigen);
            }

            comprobante.AgregarItem(linea);
        }

        if (request.Percepciones > 0)
            comprobante.SetPercepciones(request.Percepciones, currentUser.UserId);

        if (PedidoWorkflowRules.EsPedido(tipo.Codigo, tipo.Descripcion) || request.FechaEntregaCompromiso.HasValue)
            comprobante.InicializarComoPedido(request.FechaEntregaCompromiso, currentUser.UserId);

        if (request.ComprobanteOrigenId.HasValue)
            comprobante.VincularAComprobanteOrigen(request.ComprobanteOrigenId.Value, currentUser.UserId);

        await comprobanteRepo.AddAsync(comprobante, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(comprobante.Id);
    }
}
