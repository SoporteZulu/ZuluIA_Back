using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class CreateItemCommandHandler(
    IItemRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateItemCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateItemCommand request,
        CancellationToken ct)
    {
        // Validar existencia de código (con o sin sucursal)
        var existe = request.SucursalId.HasValue
            ? await repo.ExisteCodigoAsync(request.Codigo, request.SucursalId, null, ct)
            : await repo.ExisteCodigoAsync(request.Codigo, null, ct);

        if (existe)
            return Result.Failure<long>(
                $"Ya existe un ítem con el código '{request.Codigo}'.");

        // Crear el ítem con todos los parámetros fusionados
        var item = Item.Crear(
            request.Codigo,
            request.Descripcion,
            request.UnidadMedidaId,
            request.AlicuotaIvaId,
            request.MonedaId,
            request.EsProducto,
            request.EsServicio,
            request.EsFinanciero,
            request.ManejaStock,
            request.PrecioCosto,
            request.PrecioVenta,
            request.CategoriaId,
            request.StockMinimo,
            request.StockMaximo,
            request.CodigoBarras,
            request.DescripcionAdicional,
            request.CodigoAfip,
            request.SucursalId,
            currentUser.UserId);

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
            request.SucursalId,
            currentUser.UserId);

        // Actualizar precios y stock explícitamente (si la lógica lo requiere)
        item.ActualizarPrecios(
            request.PrecioCosto,
            request.PrecioVenta,
            currentUser.UserId);

        item.ActualizarStock(
            request.StockMinimo,
            request.StockMaximo,
            currentUser.UserId);

        await repo.AddAsync(item, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(item.Id);
    }
}
