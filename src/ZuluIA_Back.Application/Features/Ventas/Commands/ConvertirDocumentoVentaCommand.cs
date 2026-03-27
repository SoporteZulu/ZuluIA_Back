using MediatR;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public record ConvertirDocumentoVentaCommand(
    long ComprobanteOrigenId,
    long TipoComprobanteDestinoId,
    long? PuntoFacturacionId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    string? Observacion,
    OperacionStockVenta OperacionStock,
    OperacionCuentaCorrienteVenta OperacionCuentaCorriente
) : IRequest<Result<long>>;
