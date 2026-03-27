using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class DesimputarComprobantesMasivosCommandHandler(
    ComprobanteService comprobanteService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<DesimputarComprobantesMasivosCommand, Result<IReadOnlyList<long>>>
{
    public async Task<Result<IReadOnlyList<long>>> Handle(DesimputarComprobantesMasivosCommand request, CancellationToken ct)
    {
        var ids = new List<long>(request.ImputacionIds.Count);

        try
        {
            foreach (var imputacionId in request.ImputacionIds.Distinct())
            {
                var imputacion = await comprobanteService.DesimputarAsync(
                    imputacionId,
                    request.Fecha,
                    request.Motivo,
                    currentUser.UserId,
                    ct);

                ids.Add(imputacion.Id);
            }

            await uow.SaveChangesAsync(ct);
            return Result.Success<IReadOnlyList<long>>(ids);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<IReadOnlyList<long>>(ex.Message);
        }
    }
}
