using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public record MedioPagoInput(
    long CajaId,
    long FormaPagoId,
    long? ChequeId,
    decimal Importe,
    long MonedaId,
    decimal Cotizacion);

public record RetencionInput(
    string Tipo,
    decimal Importe,
    string? NroCertificado);

public record RegistrarPagoCommand(
    long SucursalId,
    long TerceroId,
    DateOnly Fecha,
    long MonedaId,
    decimal Cotizacion,
    string? Observacion,
    IReadOnlyList<MedioPagoInput> Medios,
    IReadOnlyList<RetencionInput> Retenciones,
    IReadOnlyList<ComprobanteAImputarInput> ComprobantesAImputar
) : IRequest<Result<long>>;