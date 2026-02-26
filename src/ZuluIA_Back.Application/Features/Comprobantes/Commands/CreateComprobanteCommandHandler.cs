using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class CreateComprobanteCommandHandler(
    IComprobanteRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
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

        var comprobante = Comprobante.Crear(
            request.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            request.Prefijo,
            request.Numero,
            request.Fecha,
            request.TerceroId,
            request.MonedaId,
            request.Cotizacion,
            currentUser.UserId);

        if (request.FechaVencimiento.HasValue)
            comprobante.SetFechaVencimiento(request.FechaVencimiento.Value);

        comprobante.SetObservacion(request.Observacion);

        foreach (var itemDto in request.Items.OrderBy(i => i.Orden))
        {
            var linea = ComprobanteItem.Crear(
                0,
                itemDto.ItemId,
                itemDto.Descripcion,
                itemDto.Cantidad,
                itemDto.PrecioUnitario,
                itemDto.DescuentoPct,
                itemDto.AlicuotaIvaId,
                itemDto.PorcentajeIva,
                itemDto.DepositoId,
                itemDto.Orden,
                itemDto.CantidadBonif);

            comprobante.AgregarItem(linea);
        }

        await repo.AddAsync(comprobante, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(comprobante.Id);
    }
}