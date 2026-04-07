using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class RegistrarDevolucionVentaCommandHandler(IMediator mediator)
    : IRequestHandler<RegistrarDevolucionVentaCommand, Result<long>>
{
    public Task<Result<long>> Handle(RegistrarDevolucionVentaCommand request, CancellationToken ct) =>
        mediator.Send(
            new RegistrarDevolucionVentaInternaCommand(
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
                request.ReingresaStock ? OperacionStockVenta.Ingreso : OperacionStockVenta.Ninguna,
                request.AcreditaCuentaCorriente ? OperacionCuentaCorrienteVenta.Credito : OperacionCuentaCorrienteVenta.Ninguna,
                request.MotivoDevolucion,
                request.ObservacionDevolucion,
                request.AutorizadorDevolucionId),
            ct);
}

internal record RegistrarDevolucionVentaInternaCommand(
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
    OperacionStockVenta OperacionStock,
    OperacionCuentaCorrienteVenta OperacionCuentaCorriente,
    MotivoDevolucion MotivoDevolucion,
    string? ObservacionDevolucion,
    long? AutorizadorDevolucionId
) : IRequest<Result<long>>;
