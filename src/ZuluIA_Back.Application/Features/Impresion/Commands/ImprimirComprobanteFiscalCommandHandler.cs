using MediatR;
using ZuluIA_Back.Application.Features.Impresion.DTOs;
using ZuluIA_Back.Application.Features.Impresion.Services;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Impresion.Commands;

public class ImprimirComprobanteFiscalCommandHandler(
    ImpresionFiscalService service)
    : IRequestHandler<ImprimirComprobanteFiscalCommand, Result<ResultadoImpresionFiscalDto>>
{
    public async Task<Result<ResultadoImpresionFiscalDto>> Handle(ImprimirComprobanteFiscalCommand request, CancellationToken ct)
    {
        try
        {
            var result = await service.ImprimirComprobanteAsync(request.ComprobanteId, request.Marca, ct);
            return Result.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<ResultadoImpresionFiscalDto>(ex.Message);
        }
    }
}
