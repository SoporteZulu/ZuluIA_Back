using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class AnularComprobanteCommandHandler(
    IComprobanteRepository repo,
    StockService stockService,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    IApplicationDbContext db)
    : IRequestHandler<AnularComprobanteCommand, Result>
{
    public async Task<Result> Handle(
        AnularComprobanteCommand request,
        CancellationToken ct)
    {
        var comprobante = await repo.GetByIdConItemsAsync(request.Id, ct);
        if (comprobante is null)
            return Result.Failure(
                $"No se encontró el comprobante con ID {request.Id}.");

        if (comprobante.Estado == EstadoComprobante.Anulado)
            return Result.Failure("El comprobante ya está anulado.");

        var tipoComp = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.TipoComprobanteId, ct);

        comprobante.Anular(currentUser.UserId);
        repo.Update(comprobante);

        // Revertir stock si corresponde
        if (request.RevertirStock && tipoComp is not null && tipoComp.AfectaStock)
        {
            foreach (var item in comprobante.Items)
            {
                var depositoId = item.DepositoId;
                if (!depositoId.HasValue) continue;

                // Invertir: si era venta (egreso) → devolver (ingreso)
                if (tipoComp.EsVenta)
                    await stockService.IngresarAsync(
                        item.ItemId, depositoId.Value,
                        item.Cantidad - item.CantidadBonificada,
                        TipoMovimientoStock.DevolucionVenta,
                        "comprobantes", comprobante.Id,
                        "Anulación de comprobante",
                        currentUser.UserId, ct);
                else
                    await stockService.EgresarAsync(
                        item.ItemId, depositoId.Value,
                        item.Cantidad - item.CantidadBonificada,
                        TipoMovimientoStock.DevolucionCompra,
                        "comprobantes", comprobante.Id,
                        "Anulación de comprobante",
                        currentUser.UserId, true, ct);
            }
        }

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
