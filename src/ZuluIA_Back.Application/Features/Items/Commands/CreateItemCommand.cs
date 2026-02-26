using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record CreateItemCommand(
    string Codigo,
    string? CodigoBarras,
    string Descripcion,
    string? DescripcionAdicional,
    long? CategoriaId,
    long UnidadMedidaId,
    long AlicuotaIvaId,
    long MonedaId,
    bool EsProducto,
    bool EsServicio,
    bool ManejaStock,
    decimal PrecioCosto,
    decimal PrecioVenta,
    decimal StockMinimo,
    decimal? StockMaximo,
    string? CodigoAfip,
    long? SucursalId
) : IRequest<Result<long>>;