using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class ConvertirPresupuestoCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    TerceroOperacionValidationService terceroOperacionValidationService)
    : IRequestHandler<ConvertirPresupuestoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        ConvertirPresupuestoCommand request,
        CancellationToken ct)
    {
        // 1. Cargar presupuesto con ítems
        var presupuesto = await db.Comprobantes
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.PresupuestoId && !x.IsDeleted, ct);

        if (presupuesto is null)
            return Result.Failure<long>($"No se encontró el presupuesto con ID {request.PresupuestoId}.");

        if (presupuesto.Estado is EstadoComprobante.Anulado or EstadoComprobante.Convertido)
            return Result.Failure<long>($"El presupuesto ya fue {presupuesto.Estado.ToString().ToLower()}.");

        if (!presupuesto.Items.Any())
            return Result.Failure<long>("El presupuesto no tiene ítems para convertir.");

        // 2. Validar tipo destino
        var tipoDestino = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TipoComprobanteDestinoId, ct);

        if (tipoDestino is null)
            return Result.Failure<long>($"No se encontró el tipo de comprobante ID {request.TipoComprobanteDestinoId}.");

        if (tipoDestino.EsVenta)
        {
            var validationError = await terceroOperacionValidationService.ValidateClienteAsync(presupuesto.TerceroId, ct);
            if (validationError is not null)
                return Result.Failure<long>(validationError);
        }

        if (tipoDestino.EsCompra)
        {
            var validationError = await terceroOperacionValidationService.ValidateProveedorAsync(presupuesto.TerceroId, ct);
            if (validationError is not null)
                return Result.Failure<long>(validationError);
        }

        // 3. Calcular número del nuevo comprobante
        short prefijo = 1;
        long numero = 1;

        if (request.PuntoFacturacionId.HasValue)
        {
            var punto = await db.PuntosFacturacion
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.PuntoFacturacionId.Value, ct);

            if (punto is null)
                return Result.Failure<long>($"No se encontró el punto de facturación ID {request.PuntoFacturacionId}.");

            prefijo = punto.Numero;
            numero  = await comprobanteRepo.GetProximoNumeroAsync(
                request.PuntoFacturacionId.Value,
                request.TipoComprobanteDestinoId, ct);
        }

        // 4. Crear nuevo comprobante desde los datos del presupuesto
        var nuevo = Comprobante.Crear(
            presupuesto.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteDestinoId,
            prefijo,
            numero,
            request.Fecha,
            request.FechaVencimiento,
            presupuesto.TerceroId,
            presupuesto.MonedaId,
            presupuesto.Cotizacion,
            request.Observacion ?? presupuesto.Observacion,
            currentUser.UserId);

        nuevo.SetComprobanteOrigen(presupuesto.Id);

        // 5. Copiar ítems
        short orden = 0;
        foreach (var item in presupuesto.Items.OrderBy(x => x.Orden))
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

        if (presupuesto.Percepciones > 0)
            nuevo.SetPercepciones(presupuesto.Percepciones, currentUser.UserId);

        // 6. Emitir el nuevo comprobante
        nuevo.Emitir(currentUser.UserId);

        await comprobanteRepo.AddAsync(nuevo, ct);

        // 7. Marcar el presupuesto como convertido
        presupuesto.MarcarComoConvertido(currentUser.UserId);

        await uow.SaveChangesAsync(ct);

        return Result.Success(nuevo.Id);
    }
}
