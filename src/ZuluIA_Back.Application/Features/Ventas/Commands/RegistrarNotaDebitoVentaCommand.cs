using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public record RegistrarNotaDebitoVentaCommand(
    long SucursalId,
    long? PuntoFacturacionId,
    long TipoComprobanteId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    long TerceroId,
    long MonedaId,
    decimal Cotizacion,
    decimal Percepciones,
    string? Observacion,
    long? ComprobanteOrigenId,
    long MotivoDebitoId,
    string? MotivoDebitoObservacion,
    IReadOnlyList<ComprobanteItemInput> Items,
    long? ListaPreciosId = null,
    long? VendedorId = null,
    long? CanalVentaId = null,
    long? CondicionPagoId = null,
    int? PlazoDias = null,
    bool Emitir = true
) : IRequest<Result<long>>;
