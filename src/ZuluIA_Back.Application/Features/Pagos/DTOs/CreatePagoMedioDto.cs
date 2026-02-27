namespace ZuluIA_Back.Application.Features.Pagos.DTOs;

public record CreatePagoMedioDto(
    long CajaId,
    long FormaPagoId,
    decimal Importe,
    long MonedaId,
    decimal Cotizacion,
    long? ChequeId = null
);
