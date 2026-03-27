using MediatR;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public record EmitirRemitosVentaMasivosCommand(
    IReadOnlyList<long> ComprobanteIds,
    OperacionCuentaCorrienteVenta OperacionCuentaCorriente
) : IRequest<Result<IReadOnlyList<long>>>;
