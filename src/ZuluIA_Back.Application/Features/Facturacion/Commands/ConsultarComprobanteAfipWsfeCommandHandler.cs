using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Application.Features.Facturacion.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class ConsultarComprobanteAfipWsfeCommandHandler(
    AfipWsfeService afipService,
    IUnitOfWork uow)
    : IRequestHandler<ConsultarComprobanteAfipWsfeCommand, Result<AfipWsfeOperacionDto>>
{
    public async Task<Result<AfipWsfeOperacionDto>> Handle(ConsultarComprobanteAfipWsfeCommand request, CancellationToken ct)
    {
        try
        {
            var result = await afipService.ConsultarAsync(request.ComprobanteId, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<AfipWsfeOperacionDto>(ex.Message);
        }
    }
}
