using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record UpdateItemCommand(
    long Id,
    string Descripcion,
    string? DescripcionAdicional,
    string? CodigoBarras,
    long? CategoriaId,
    decimal PrecioCosto,
    decimal PrecioVenta,
    decimal StockMinimo,
    decimal? StockMaximo,
    string? CodigoAfip
) : IRequest<Result>;