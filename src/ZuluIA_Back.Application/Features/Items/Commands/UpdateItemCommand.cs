using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record UpdateItemCommand(
    long Id,
    string Descripcion,
    string? DescripcionAdicional,
    string? CodigoBarras,
    long UnidadMedidaId,
    long AlicuotaIvaId,
    long MonedaId,
    bool EsProducto,
    bool EsServicio,
    bool EsFinanciero,
    bool ManejaStock,
    long? CategoriaId,
    decimal PrecioCosto,
    decimal PrecioVenta,
    decimal StockMinimo,
    decimal? StockMaximo,
    string? CodigoAfip
) : IRequest<Result>;
