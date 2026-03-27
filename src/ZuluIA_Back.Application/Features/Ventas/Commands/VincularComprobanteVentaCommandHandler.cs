using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class VincularComprobanteVentaCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<VincularComprobanteVentaCommand, Result>
{
    public async Task<Result> Handle(VincularComprobanteVentaCommand request, CancellationToken ct)
    {
        var origen = await comprobanteRepo.GetByIdAsync(request.ComprobanteOrigenId, ct);
        if (origen is null)
            return Result.Failure($"No se encontró el comprobante origen ID {request.ComprobanteOrigenId}.");

        var destino = await comprobanteRepo.GetByIdAsync(request.ComprobanteDestinoId, ct);
        if (destino is null)
            return Result.Failure($"No se encontró el comprobante destino ID {request.ComprobanteDestinoId}.");

        if (origen.TerceroId != destino.TerceroId)
            return Result.Failure("El origen y el destino deben pertenecer al mismo tercero.");

        if (origen.Estado == EstadoComprobante.Anulado || destino.Estado == EstadoComprobante.Anulado)
            return Result.Failure("No se pueden vincular comprobantes anulados.");

        var tipoOrigen = await db.TiposComprobante.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == origen.TipoComprobanteId, ct);
        var tipoDestino = await db.TiposComprobante.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == destino.TipoComprobanteId, ct);

        if (tipoOrigen is null || tipoDestino is null || !tipoOrigen.EsVenta || !tipoDestino.EsVenta)
            return Result.Failure("Ambos comprobantes deben pertenecer al circuito de ventas.");

        destino.VincularAComprobanteOrigen(origen.Id, currentUser.UserId);
        comprobanteRepo.Update(destino);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
