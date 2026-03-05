namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ComprobanteItemDto
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public string ItemCodigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal CantidadBonificada { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoPct { get; set; }
    public long AlicuotaIvaId { get; set; }
    public decimal PorcentajeIva { get; set; }
    public decimal SubtotalNeto { get; set; }
    public decimal IvaImporte { get; set; }
    public decimal TotalLinea { get; set; }
    public long? DepositoId { get; set; }
    public string? DepositoDescripcion { get; set; }
    public short Orden { get; set; }
    public bool EsGravado { get; set; }
}
