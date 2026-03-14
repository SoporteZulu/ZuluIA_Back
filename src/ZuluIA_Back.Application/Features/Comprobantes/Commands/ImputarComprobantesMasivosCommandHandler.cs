using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class ImputarComprobantesMasivosCommandHandler(
    ComprobanteService comprobanteService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<ImputarComprobantesMasivosCommand, Result<IReadOnlyList<long>>>
{
    public async Task<Result<IReadOnlyList<long>>> Handle(
        ImputarComprobantesMasivosCommand request,
        CancellationToken ct)
    {
        if (!request.Items.Any())
            return Result.Failure<IReadOnlyList<long>>("Debe incluir al menos una imputación.");

        var ids = new List<long>(request.Items.Count);

        try
        {
            foreach (var item in request.Items)
            {
                var imputacion = await comprobanteService.ImputarAsync(
                    item.ComprobanteOrigenId,
                    item.ComprobanteDestinoId,
                    item.Importe,
                    request.Fecha,
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
