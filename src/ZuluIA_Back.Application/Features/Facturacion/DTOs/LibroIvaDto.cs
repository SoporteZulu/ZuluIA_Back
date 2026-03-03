namespace ZuluIA_Back.Application.Features.Facturacion.DTOs;

public class LineaLibroIvaDto
{
    public DateOnly Fecha { get; set; }
    public string TipoComprobante { get; set; } = string.Empty;
    public short Prefijo { get; set; }
    public long Numero { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string Cuit { get; set; } = string.Empty;
    public string CondicionIva { get; set; } = string.Empty;
    public decimal Neto { get; set; }
    public decimal Iva { get; set; }
    public decimal Percepciones { get; set; }
    public decimal Total { get; set; }
}

public class LibroIvaDto
{
    public DateOnly Desde { get; set; }
    public DateOnly Hasta { get; set; }
    public long SucursalId { get; set; }
    public string TipoLibro { get; set; } = string.Empty;
    public int CantidadComprobantes { get; set; }
    public decimal TotalNeto { get; set; }
    public decimal TotalIva { get; set; }
    public decimal TotalPercepciones { get; set; }
    public decimal TotalGeneral { get; set; }
    public IReadOnlyList<LineaLibroIvaDto> Lineas { get; set; } = [];
}