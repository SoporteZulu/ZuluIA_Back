using MediatR;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class EmitirRemitosVentaMasivosCommandHandler(IMediator mediator)
    : IRequestHandler<EmitirRemitosVentaMasivosCommand, Result<IReadOnlyList<long>>>
{
    public async Task<Result<IReadOnlyList<long>>> Handle(EmitirRemitosVentaMasivosCommand request, CancellationToken ct)
    {
        var emitidos = new List<long>();

        foreach (var comprobanteId in request.ComprobanteIds.Distinct())
        {
            var result = await mediator.Send(
                new EmitirDocumentoVentaCommand(
                    comprobanteId,
                    OperacionStockVenta.Egreso,
                    request.OperacionCuentaCorriente),
                ct);

            if (!result.IsSuccess)
                return Result.Failure<IReadOnlyList<long>>(result.Error ?? "No se pudo emitir uno de los remitos solicitados.");

            emitidos.Add(result.Value);
        }

        return Result.Success<IReadOnlyList<long>>(emitidos);
    }
}
