using MediatR;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public record EmitirDocumentoVentaCommand(
    long ComprobanteId,
    OperacionStockVenta OperacionStock,
    OperacionCuentaCorrienteVenta OperacionCuentaCorriente
) : IRequest<Result<long>>;
