namespace ZuluIA_Back.Application.Features.Compras.DTOs;

public class CotizacionCompraListDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long? RequisicionId { get; set; }
    public long ProveedorId { get; set; }
    public string ProveedorRazonSocial { get; set; } = string.Empty;
    public string Proveedor => ProveedorRazonSocial;
    public string? RequisicionReferencia { get; set; }
    public DateOnly Fecha { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string EstadoLegacy { get; set; } = string.Empty;
    public int CantidadItems { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CotizacionCompraDto : CotizacionCompraListDto
{
    public string? Observacion { get; set; }
    public IReadOnlyList<CotizacionCompraItemDto> Items { get; set; } = [];
}

public class CotizacionCompraItemDto
{
    public long Id { get; set; }
    public long? ItemId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public decimal Subtotal => Total;
}
