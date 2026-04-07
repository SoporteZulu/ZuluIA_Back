using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

/// <summary>
/// Comando para actualizar el porcentaje de ganancia de un item.
/// El precio de venta se recalcula automáticamente si hay porcentaje configurado.
/// </summary>
public record UpdateItemPorcentajeGananciaCommand(
    long ItemId,
    decimal? PorcentajeGanancia
) : IRequest<Result>;
