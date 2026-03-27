using MediatR;
using ZuluIA_Back.Application.Features.Compras.Common;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public record EmitirDocumentoCompraCommand(
    long ComprobanteId,
    OperacionStockCompra OperacionStock,
    OperacionCuentaCorrienteCompra OperacionCuentaCorriente
) : IRequest<Result<long>>;
