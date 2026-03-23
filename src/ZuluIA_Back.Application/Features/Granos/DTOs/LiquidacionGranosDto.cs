namespace ZuluIA_Back.Application.Features.Granos.DTOs;

public record LiquidacionGranosListDto(
    long Id,
    long SucursalId,
    long TerceroId,
    string Producto,
    DateOnly Fecha,
    decimal Cantidad,
    decimal PrecioBase,
    decimal ValorNeto,
    string Estado);

public record LiquidacionGranosDto(
    long Id,
    long SucursalId,
    long TerceroId,
    string Producto,
    DateOnly Fecha,
    decimal Cantidad,
    decimal PrecioBase,
    decimal Deducciones,
    decimal ValorNeto,
    string Estado,
    long? ComprobanteId,
    IReadOnlyCollection<ConceptoDto> Conceptos);

public record ConceptoDto(long Id, string Concepto, decimal Importe, bool EsDeduccion);

public record CertificacionGranosDto(
    long Id,
    long LiquidacionId,
    string NroCertificado,
    DateOnly FechaEmision,
    decimal PesoNeto,
    decimal Humedad,
    decimal Impureza,
    string? CalidadObservaciones);
