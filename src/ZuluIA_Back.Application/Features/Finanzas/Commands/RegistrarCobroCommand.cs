using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public record MedioCobroInput(
    long CajaId,
    long FormaPagoId,
    long? ChequeId,
    decimal Importe,
    long MonedaId,
    decimal Cotizacion);

public record ComprobanteAImputarInput(
    long ComprobanteId,
    decimal Importe);

public record RegistrarCobroCommand(
    long SucursalId,
    long TerceroId,
    DateOnly Fecha,
    long MonedaId,
    decimal Cotizacion,
    string? Observacion,
    IReadOnlyList<MedioCobroInput> Medios,
    IReadOnlyList<ComprobanteAImputarInput> ComprobantesAImputar
) : IRequest<Result<long>>;