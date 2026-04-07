using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Application.Features.Facturacion.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class RefreshEstadoAfipWsfeCommandHandler(
    AfipWsfeService afipService,
    IUnitOfWork uow)
    : IRequestHandler<RefreshEstadoAfipWsfeCommand, Result<AfipWsfeOperacionDto>>
{
    public async Task<Result<AfipWsfeOperacionDto>> Handle(RefreshEstadoAfipWsfeCommand request, CancellationToken ct)
    {
        try
        {
            var result = await afipService.RefreshEstadoAsync(request.ComprobanteId, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<AfipWsfeOperacionDto>(ex.Message);
        }
    }
}
