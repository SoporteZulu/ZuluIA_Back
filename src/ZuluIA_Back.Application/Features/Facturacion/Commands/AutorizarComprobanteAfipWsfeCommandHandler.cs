using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Application.Features.Facturacion.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class AutorizarComprobanteAfipWsfeCommandHandler(
    AfipWsfeService afipService,
    IUnitOfWork uow)
    : IRequestHandler<AutorizarComprobanteAfipWsfeCommand, Result<AfipWsfeOperacionDto>>
{
    public async Task<Result<AfipWsfeOperacionDto>> Handle(AutorizarComprobanteAfipWsfeCommand request, CancellationToken ct)
    {
        try
        {
            var result = request.UsarCaea
                ? await afipService.SolicitarCaeaAsync(request.ComprobanteId, ct)
                : await afipService.SolicitarCaeAsync(request.ComprobanteId, ct);

            await uow.SaveChangesAsync(ct);
            return Result.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<AfipWsfeOperacionDto>(ex.Message);
        }
    }
}
