using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

/// <summary>
/// Convierte un presupuesto (comprobante tipo presupuesto) en un comprobante definitivo
/// de un tipo destino (ej.: Factura A, Remito, etc.).
/// Equivale al flujo "Convertir Presupuesto" de frmPreFacturaVenta del VB6.
/// </summary>
public record ConvertirPresupuestoCommand(
    long PresupuestoId,
    long TipoComprobanteDestinoId,
    long? PuntoFacturacionId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    string? Observacion
) : IRequest<Result<long>>;
