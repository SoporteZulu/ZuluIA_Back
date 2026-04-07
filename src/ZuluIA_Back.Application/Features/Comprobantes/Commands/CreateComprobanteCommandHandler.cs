using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class CreateComprobanteCommandHandler(
    IComprobanteRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    IApplicationDbContext db,
    TerceroOperacionValidationService terceroOperacionValidationService)
    : IRequestHandler<CreateComprobanteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateComprobanteCommand request, CancellationToken ct)
    {
        var existente = await repo.GetByNumeroAsync(
            request.SucursalId,
            request.TipoComprobanteId,
            request.Prefijo,
            request.Numero,
            ct);

        if (existente is not null)
            return Result.Failure<long>($"Ya existe el comprobante {request.Prefijo:D4}-{request.Numero:D8}.");

        var tipoComprobante = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TipoComprobanteId, ct);

        if (tipoComprobante is null)
            return Result.Failure<long>($"No se encontró el tipo de comprobante ID {request.TipoComprobanteId}.");

        if (tipoComprobante.EsVenta)
        {
            var validationError = await terceroOperacionValidationService.ValidateClienteAsync(request.TerceroId, ct);
            if (validationError is not null)
                return Result.Failure<long>(validationError);

            var descuentoValidationError = await ClienteDescuentoMaximoValidator.ValidateAsync(
                db,
                request.TerceroId,
                request.Items.Select(x => Convert.ToDecimal(x.DescuentoPct)),
                ct);

            if (descuentoValidationError is not null)
                return Result.Failure<long>(descuentoValidationError);
        }

        if (tipoComprobante.EsCompra)
        {
            var validationError = await terceroOperacionValidationService.ValidateProveedorAsync(request.TerceroId, ct);
            if (validationError is not null)
                return Result.Failure<long>(validationError);
        }

        var comprobante = Comprobante.Crear(
            request.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            request.Prefijo,
            request.Numero,
            request.Fecha,
            request.FechaVencimiento,
            request.TerceroId,
            request.MonedaId,
            request.Cotizacion,
            request.Observacion,
            currentUser.UserId);

        if (request.FechaVencimiento.HasValue)
            comprobante.SetFechaVencimiento(request.FechaVencimiento.Value, currentUser.UserId);

        //comprobante.SetObservacion(request.Observacion);

        foreach (var itemDto in request.Items.OrderBy(i => i.Orden))
        {
            var linea = ComprobanteItem.Crear(
                0,
                itemDto.ItemId,
                itemDto.Descripcion,
                itemDto.Cantidad,
                itemDto.CantidadBonif,
                itemDto.PrecioUnitario,
                itemDto.DescuentoPct,
                itemDto.AlicuotaIvaId,
                itemDto.PorcentajeIva,
                itemDto.DepositoId,
                itemDto.Orden);

            linea.SetDatosDocumentoOrigen(
                itemDto.CantidadDocumentoOrigen,
                itemDto.PrecioDocumentoOrigen);

            comprobante.AgregarItem(linea);
        }

        await repo.AddAsync(comprobante, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var impuesto in BuildImpuestos(comprobante))
            db.ComprobantesImpuestos.Add(impuesto);

        await uow.SaveChangesAsync(ct);

        return Result.Success(comprobante.Id);
    }

    private static IReadOnlyCollection<ComprobanteImpuesto> BuildImpuestos(Comprobante comprobante)
    {
        return comprobante.Items
            .Where(x => x.IvaImporte > 0)
            .GroupBy(x => new { x.AlicuotaIvaId, x.PorcentajeIva })
            .Select(group => ComprobanteImpuesto.Crear(
                comprobante.Id,
                group.Key.AlicuotaIvaId,
                group.Key.PorcentajeIva,
                Math.Round(group.Sum(x => x.SubtotalNeto), 2),
                Math.Round(group.Sum(x => x.IvaImporte), 2)))
            .ToList();
    }
}