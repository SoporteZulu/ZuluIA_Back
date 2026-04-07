using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.PuntoVenta.Enums;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public record RegistrarComprobanteFiscalAlternativoCommand(
    CanalOperacionPuntoVenta Canal,
    TipoFlujoFiscalAlternativo Flujo,
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
    IReadOnlyList<ComprobanteItemInput> Items,
    long? TimbradoFiscalId = null,
    string? CodigoSeguridad = null,
    string? RequestPayload = null,
    string? ResponsePayload = null,
    bool Confirmada = true) : IRequest<Result<long>>;
