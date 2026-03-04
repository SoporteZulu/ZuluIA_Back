using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Queries;

public class GetItemByIdQueryHandler(
    IItemRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetItemByIdQuery, ItemDto?>
{
    public async Task<ItemDto?> Handle(GetItemByIdQuery request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.Id, ct);
        if (item is null) return null;

        var categoria = item.CategoriaId.HasValue
            ? await db.CategoriasItems
                .AsNoTracking()
                .Where(x => x.Id == item.CategoriaId.Value)
                .Select(x => new { x.Descripcion })
                .FirstOrDefaultAsync(ct)
            : null;

        var unidad = await db.UnidadesMedida
            .AsNoTracking()
            .Where(x => x.Id == item.UnidadMedidaId)
            .Select(x => new { x.Descripcion })
            .FirstOrDefaultAsync(ct);

        var alicuota = await db.AlicuotasIva
            .AsNoTracking()
            .Where(x => x.Id == item.AlicuotaIvaId)
            .Select(x => new { x.Porcentaje })
            .FirstOrDefaultAsync(ct);

        return new ItemDto
        {
            Id                      = item.Id,
            Codigo                  = item.Codigo,
            CodigoBarras            = item.CodigoBarras,
            Descripcion             = item.Descripcion,
            DescripcionAdicional    = item.DescripcionAdicional,
            CategoriaId             = item.CategoriaId,
            CategoriaDescripcion    = categoria?.Descripcion,
            UnidadMedidaId          = item.UnidadMedidaId,
            UnidadMedidaDescripcion = unidad?.Descripcion,
            AlicuotaIvaId           = item.AlicuotaIvaId,
            AlicuotaIvaPorcentaje   = alicuota?.Porcentaje ?? 0,
            MonedaId                = item.MonedaId,
            EsProducto              = item.EsProducto,
            EsServicio              = item.EsServicio,
            EsFinanciero            = item.EsFinanciero,
            ManejaStock             = item.ManejaStock,
            PrecioCosto             = item.PrecioCosto,
            PrecioVenta             = item.PrecioVenta,
            StockMinimo             = item.StockMinimo,
            StockMaximo             = item.StockMaximo,
            CodigoAfip              = item.CodigoAfip,
            SucursalId              = item.SucursalId,
            Activo                  = item.Activo,
            CreatedAt               = item.CreatedAt,
            UpdatedAt               = item.UpdatedAt
        };
    }
}
