using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Compras.Common;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class RegistrarDevolucionCompraCommandHandler(IMediator mediator)
    : IRequestHandler<RegistrarDevolucionCompraCommand, Result<long>>
{
    public Task<Result<long>> Handle(RegistrarDevolucionCompraCommand request, CancellationToken ct) =>
        mediator.Send(
            new RegistrarDevolucionCompraInternaCommand(
                request.SucursalId,
                request.PuntoFacturacionId,
                request.TipoComprobanteId,
                request.Fecha,
                request.FechaVencimiento,
                request.TerceroId,
                request.MonedaId,
                request.Cotizacion,
                request.Percepciones,
                request.Observacion,
                request.ComprobanteOrigenId,
                request.Items,
                request.EgresoStock ? OperacionStockCompra.Egreso : OperacionStockCompra.Ninguna,
                request.AcreditaCuentaCorriente ? OperacionCuentaCorrienteCompra.Credito : OperacionCuentaCorrienteCompra.Ninguna),
            ct);
}

internal record RegistrarDevolucionCompraInternaCommand(
    long SucursalId,
    long? PuntoFacturacionId,
    long TipoComprobanteId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    long TerceroId,
    long MonedaId,
    decimal Cotizacion,
    decimal Percepciones,
    string? Observacion,
    long? ComprobanteOrigenId,
    IReadOnlyList<ComprobanteItemInput> Items,
    OperacionStockCompra OperacionStock,
    OperacionCuentaCorrienteCompra OperacionCuentaCorriente
) : IRequest<Result<long>>;
