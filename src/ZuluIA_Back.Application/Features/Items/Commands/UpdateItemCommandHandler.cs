using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class UpdateItemCommandHandler(
    IItemRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateItemCommand, Result>
{
    public async Task<Result> Handle(UpdateItemCommand request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.Id, ct);
        if (item is null)
            return Result.Failure($"No se encontró el ítem con ID {request.Id}.");

        // Actualizar todos los campos relevantes
        item.Actualizar(
            request.Descripcion,
            request.DescripcionAdicional,
            request.CodigoBarras,
            request.UnidadMedidaId,
            request.AlicuotaIvaId,
            request.MonedaId,
            request.EsProducto,
            request.EsServicio,
            request.EsFinanciero,
            request.ManejaStock,
            request.CategoriaId,
            request.CodigoAfip,
            request.StockMinimo,
            request.StockMaximo,
            item.SucursalId,
            currentUser.UserId);

        // Actualizar precios si cambiaron
        if (request.PrecioCosto != item.PrecioCosto || request.PrecioVenta != item.PrecioVenta)
            item.ActualizarPrecios(request.PrecioCosto, request.PrecioVenta, currentUser.UserId);

        // Actualizar stock si cambió
        if (request.StockMinimo != item.StockMinimo || request.StockMaximo != item.StockMaximo)
            item.ActualizarStock(request.StockMinimo, request.StockMaximo, currentUser.UserId);

        repo.Update(item);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
