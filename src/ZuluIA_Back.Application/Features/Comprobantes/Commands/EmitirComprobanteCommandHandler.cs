using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class EmitirComprobanteCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IPeriodoIvaRepository periodoRepo,
    StockService stockService,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    IApplicationDbContext db)
    : IRequestHandler<EmitirComprobanteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        EmitirComprobanteCommand request,
        CancellationToken ct)
    {
        // 1. Validar período IVA abierto
        var periodoAbierto = await periodoRepo.EstaAbiertoPeriodoAsync(
            request.SucursalId, request.Fecha, ct);

        if (!periodoAbierto)
            return Result.Failure<long>(
                $"El período IVA para {request.Fecha:yyyy-MM} está cerrado.");

        // 2. Validar ítems
        if (!request.Items.Any())
            return Result.Failure<long>(
                "El comprobante debe tener al menos un ítem.");

        // 3. Obtener tipo de comprobante
        var tipoComp = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TipoComprobanteId, ct);

        if (tipoComp is null)
            return Result.Failure<long>(
                $"No se encontró el tipo de comprobante ID {request.TipoComprobanteId}.");

        // 4. Obtener próximo número
        short prefijo = 1;
        long numero = 1;

        if (request.PuntoFacturacionId.HasValue)
        {
            var punto = await db.PuntosFacturacion
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.PuntoFacturacionId.Value, ct);

            if (punto is null)
                return Result.Failure<long>(
                    $"No se encontró el punto de facturación ID {request.PuntoFacturacionId}.");

            prefijo = punto.Numero;
            numero  = await comprobanteRepo.GetProximoNumeroAsync(
                request.PuntoFacturacionId.Value,
                request.TipoComprobanteId, ct);
        }

        // 5. Crear comprobante
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

        // 6. Agregar ítems
        var alicuotaIds = request.Items.Select(x => x.AlicuotaIvaId).Distinct().ToList();
        var alicuotas = await db.AlicuotasIva
            .AsNoTracking()
            .Where(x => alicuotaIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        short orden = 0;
        foreach (var itemInput in request.Items)
        {
            if (!alicuotas.TryGetValue(itemInput.AlicuotaIvaId, out var alicuota))
                return Result.Failure<long>(
                    $"No se encontró la alícuota de IVA ID {itemInput.AlicuotaIvaId}.");

            var itemDb = await db.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == itemInput.ItemId, ct);

            if (itemDb is null)
                return Result.Failure<long>(
                    $"No se encontró el ítem ID {itemInput.ItemId}.");

            var descripcion = itemInput.Descripcion ?? itemDb.Descripcion;

            var linea = ComprobanteItem.Crear(
                0, // Se asigna al persistir
                itemInput.ItemId,
                descripcion,
                itemInput.Cantidad,
                itemInput.CantidadBonificada,
                itemInput.PrecioUnitario,
                itemInput.DescuentoPct,
                itemInput.AlicuotaIvaId,
                alicuota.Porcentaje,
                itemInput.DepositoId,
                orden++);

            comprobante.AgregarItem(linea);
        }

        // 7. Percepciones
        if (request.Percepciones > 0)
            comprobante.SetPercepciones(request.Percepciones, currentUser.UserId);

        // 8. Emitir
        comprobante.Emitir(currentUser.UserId);

        // 9. Persistir
        await comprobanteRepo.AddAsync(comprobante, ct);

        // 10. Afectar stock si corresponde
        if (request.AfectaStock && tipoComp.AfectaStock)
        {
            foreach (var itemInput in request.Items)
            {
                var depositoId = itemInput.DepositoId;
                if (!depositoId.HasValue)
                {
                    var defDeposito = await db.Depositos
                        .AsNoTracking()
                        .Where(x => x.SucursalId == request.SucursalId &&
                                    x.EsDefault  == true &&
                                    x.Activo     == true)
                        .Select(x => x.Id)
                        .FirstOrDefaultAsync(ct);

                    depositoId = defDeposito == 0 ? null : defDeposito;
                }

                if (!depositoId.HasValue) continue;

                var tipoMov = tipoComp.EsVenta
                    ? TipoMovimientoStock.VentaDespacho
                    : TipoMovimientoStock.CompraRecepcion;

                if (tipoComp.EsVenta)
                    await stockService.EgresarAsync(
                        itemInput.ItemId, depositoId.Value,
                        itemInput.Cantidad - itemInput.CantidadBonificada,
                        tipoMov, "comprobantes", 0,
                        null, currentUser.UserId, false, ct);
                else
                    await stockService.IngresarAsync(
                        itemInput.ItemId, depositoId.Value,
                        itemInput.Cantidad - itemInput.CantidadBonificada,
                        tipoMov, "comprobantes", 0,
                        null, currentUser.UserId, ct);
            }
        }

        await uow.SaveChangesAsync(ct);
        return Result.Success(comprobante.Id);
    }
}
