using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class ImputarComprobanteCommandHandler(
    ComprobanteService comprobanteService,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    IApplicationDbContext db)
    : IRequestHandler<ImputarComprobanteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        ImputarComprobanteCommand request,
        CancellationToken ct)
    {
        try
        {
            if (request.TipoComprobanteOrigenId.HasValue && request.TipoComprobanteDestinoId.HasValue)
            {
                var tipos = await db.TiposComprobante
                    .AsNoTracking()
                    .Where(x => x.Id == request.TipoComprobanteOrigenId.Value || x.Id == request.TipoComprobanteDestinoId.Value)
                    .ToDictionaryAsync(x => x.Id, ct);

                if (!tipos.TryGetValue(request.TipoComprobanteOrigenId.Value, out var tipoOrigen) ||
                    !tipos.TryGetValue(request.TipoComprobanteDestinoId.Value, out var tipoDestino))
                    return Result.Failure<long>("No se pudieron validar los tipos de documento enviados.");

                if (tipoOrigen.EsVenta != tipoDestino.EsVenta || tipoOrigen.EsCompra != tipoDestino.EsCompra)
                    return Result.Failure<long>("Solo se pueden imputar comprobantes del mismo circuito documental.");
            }

            var imputacion = await comprobanteService.ImputarAsync(
                request.ComprobanteOrigenId,
                request.ComprobanteDestinoId,
                request.Importe,
                request.Fecha,
                request.TipoComprobanteOrigenId,
                request.TipoComprobanteDestinoId,
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