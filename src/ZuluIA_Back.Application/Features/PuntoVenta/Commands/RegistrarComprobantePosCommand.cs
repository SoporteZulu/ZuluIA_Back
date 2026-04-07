using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public record RegistrarComprobantePosCommand(
    long SucursalId,
    long PuntoFacturacionId,
    long TipoComprobanteId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    long TerceroId,
    long MonedaId,
    decimal Cotizacion,
    decimal Percepciones,
    string? Observacion,
    bool AfectaStock,
    string? ReferenciaExterna,
    IReadOnlyList<ComprobanteItemInput> Items) : IRequest<Result<long>>;
