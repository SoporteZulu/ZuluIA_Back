using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class ImputarComprobanteCommandHandler(
    ComprobanteService comprobanteService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<ImputarComprobanteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        ImputarComprobanteCommand request,
        CancellationToken ct)
    {
        var imputacion = await comprobanteService.ImputarAsync(
            request.ComprobanteOrigenId,
            request.ComprobanteDestinoId,
            request.Importe,
            request.Fecha,
            currentUser.UserId,
            ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success(imputacion.Id);
    }
}