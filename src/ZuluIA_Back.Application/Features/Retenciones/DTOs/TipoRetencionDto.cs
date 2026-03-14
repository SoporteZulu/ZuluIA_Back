namespace ZuluIA_Back.Application.Features.Retenciones.DTOs;

public record TipoRetencionDto(
    long Id,
    string Descripcion,
    string Regimen,
    decimal MinimoNoImponible,
    bool AcumulaPago,
    long? TipoComprobanteId,
    long? ItemId,
    bool Activo,
    IReadOnlyList<EscalaRetencionDto> Escalas
);

public record EscalaRetencionDto(
    long Id,
    string Descripcion,
    decimal ImporteDesde,
    decimal ImporteHasta,
    decimal Porcentaje
);

public record EscalaRetencionInputDto(
    string Descripcion,
    decimal ImporteDesde,
    decimal ImporteHasta,
    decimal Porcentaje
);
