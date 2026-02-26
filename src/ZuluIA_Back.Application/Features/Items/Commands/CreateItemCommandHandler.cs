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
    public async Task<Result<long>> Handle(CreateItemCommand request, CancellationToken ct)
    {
        if (await repo.ExisteCodigoAsync(request.Codigo, request.SucursalId, null, ct))
            return Result.Failure<long>($"Ya existe un ítem con el código '{request.Codigo}'.");

        var item = Item.Crear(
            request.Codigo,
            request.Descripcion,
            request.UnidadMedidaId,
            request.AlicuotaIvaId,
            request.MonedaId,
            request.EsProducto,
            request.EsServicio,
            request.ManejaStock,
            request.PrecioCosto,
            request.PrecioVenta,
            request.SucursalId,
            currentUser.UserId);

        item.Actualizar(
            request.Descripcion,
            request.DescripcionAdicional,
            request.CodigoBarras,
            request.CategoriaId,
            request.StockMinimo,
            request.StockMaximo,
            request.CodigoAfip,
            currentUser.UserId);

        await repo.AddAsync(item, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(item.Id);
    }
}