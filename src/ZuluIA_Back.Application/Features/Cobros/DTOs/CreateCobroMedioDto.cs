namespace ZuluIA_Back.Application.Features.Cobros.DTOs;

public record CreateCobroMedioDto(
    long CajaId,
    long FormaPagoId,
    long Importe,
    long MonedaId,
    long Cotizacion,
    long? ChequeId = null
);