namespace ZuluIA_Back.Application.Features.Contabilidad.DTOs;

public record CreateAsientoLineaDto(
    long CuentaId,
    decimal Debe,
    decimal Haber,
    string? Descripcion,
    short Orden,
    long? CentroCostoId = null
);