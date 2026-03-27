using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class ImputarComprobantesMasivosCommandHandler(
    ComprobanteService comprobanteService,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    IApplicationDbContext db)
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
                if (item.TipoComprobanteOrigenId.HasValue && item.TipoComprobanteDestinoId.HasValue)
                {
                    var tipos = await db.TiposComprobante
                        .AsNoTracking()
                        .Where(x => x.Id == item.TipoComprobanteOrigenId.Value || x.Id == item.TipoComprobanteDestinoId.Value)
                        .ToDictionaryAsync(x => x.Id, ct);

                    if (!tipos.TryGetValue(item.TipoComprobanteOrigenId.Value, out var tipoOrigen) ||
                        !tipos.TryGetValue(item.TipoComprobanteDestinoId.Value, out var tipoDestino))
                        return Result.Failure<IReadOnlyList<long>>("No se pudieron validar los tipos de documento enviados.");

                    if (tipoOrigen.EsVenta != tipoDestino.EsVenta || tipoOrigen.EsCompra != tipoDestino.EsCompra)
                        return Result.Failure<IReadOnlyList<long>>("Solo se pueden imputar comprobantes del mismo circuito documental.");
                }

                var imputacion = await comprobanteService.ImputarAsync(
                    item.ComprobanteOrigenId,
                    item.ComprobanteDestinoId,
                    item.Importe,
                    request.Fecha,
                    item.TipoComprobanteOrigenId,
                    item.TipoComprobanteDestinoId,
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
