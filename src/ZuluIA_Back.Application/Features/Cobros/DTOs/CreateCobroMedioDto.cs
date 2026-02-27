namespace ZuluIA_Back.Application.Features.Cobros.DTOs;

public record CreateCobroMedioDto(
    long CajaId,
    long FormaPagoId,
    decimal Importe,
    long MonedaId,
    decimal Cotizacion,
    long? ChequeId = null
);