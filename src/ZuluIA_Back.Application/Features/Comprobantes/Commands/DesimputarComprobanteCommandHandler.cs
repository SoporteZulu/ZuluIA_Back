using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class DesimputarComprobanteCommandHandler(
    ComprobanteService comprobanteService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<DesimputarComprobanteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(DesimputarComprobanteCommand request, CancellationToken ct)
    {
        try
        {
            var imputacion = await comprobanteService.DesimputarAsync(
                request.ImputacionId,
                request.Fecha,
                request.Motivo,
                currentUser.UserId,
                ct);

            await uow.SaveChangesAsync(ct);
            return Result.Success(imputacion.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
