namespace ZuluIA_Back.Domain.Services;

/// <summary>
/// Value object que representa una línea del Libro IVA.
/// </summary>
public record LineaLibroIva(
    DateOnly Fecha,
    string TipoComprobante,
    short Prefijo,
    long Numero,
    string RazonSocial,
    string Cuit,
    string CondicionIva,
    decimal Neto,
    decimal Iva,
    decimal Percepciones,
    decimal Total
);

/// <summary>
/// Value object que representa el resumen del Libro IVA por período.
/// </summary>
public record ResumenLibroIva(
    DateOnly Periodo,
    int CantidadComprobantes,
    decimal TotalNeto,
    decimal TotalIva,
    decimal TotalPercepciones,
    decimal TotalGeneral,
    IReadOnlyList<LineaLibroIva> Lineas
);