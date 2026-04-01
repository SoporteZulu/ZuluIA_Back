using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

/// <summary>
/// Comando para actualizar la configuración de ventas de un item (Fase 1).
/// </summary>
public record UpdateItemConfiguracionVentasCommand(
    long ItemId,
    bool AplicaVentas,
    bool AplicaCompras,
    decimal? PorcentajeMaximoDescuento,
    bool EsRpt
) : IRequest<Result>;
