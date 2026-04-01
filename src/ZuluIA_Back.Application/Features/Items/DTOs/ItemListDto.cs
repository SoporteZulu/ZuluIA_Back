namespace ZuluIA_Back.Application.Features.Items.DTOs;

public class ItemListDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? CodigoAlternativo { get; set; }
    public string? CodigoBarras { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? DescripcionAdicional { get; set; }
    public long? CategoriaId { get; set; }
    public string? CategoriaDescripcion { get; set; }
    public long? MarcaId { get; set; }
    public string? MarcaDescripcion { get; set; }
    public long UnidadMedidaId { get; set; }
    public string? UnidadMedidaDescripcion { get; set; }
    public long AlicuotaIvaId { get; set; }
    public string? AlicuotaIvaDescripcion { get; set; }
    public decimal AlicuotaIvaPorcentaje { get; set; }
    public long? AlicuotaIvaCompraId { get; set; }
    public string? AlicuotaIvaCompraDescripcion { get; set; }
    public decimal? AlicuotaIvaCompraPorcentaje { get; set; }
    public long? ImpuestoInternoId { get; set; }
    public string? ImpuestoInternoDescripcion { get; set; }
    public long MonedaId { get; set; }
    public string? MonedaSimbol { get; set; }
    public bool EsProducto { get; set; }
    public bool EsServicio { get; set; }
    public bool EsFinanciero { get; set; }
    public bool ManejaStock { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal Stock { get; set; }
    public decimal StockDisponible { get; set; }
    public decimal StockComprometido { get; set; }
    public decimal StockReservado { get; set; }
    public decimal StockEnTransito { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
    public decimal? PuntoReposicion { get; set; }
    public decimal? StockSeguridad { get; set; }
    public decimal? Peso { get; set; }
    public decimal? Volumen { get; set; }
    public bool EsTrazable { get; set; }
    public bool PermiteFraccionamiento { get; set; }
    public int? DiasVencimientoLimite { get; set; }
    public long? DepositoDefaultId { get; set; }
    public string? DepositoDefaultDescripcion { get; set; }
    public string? CodigoAfip { get; set; }
    public long? SucursalId { get; set; }
    public bool Activo { get; set; }
    
    // ── Fase 1: Campos adicionales para listado ──────────────────────────────
    public bool AplicaVentas { get; set; }
    public bool AplicaCompras { get; set; }
    public decimal? PorcentajeGanancia { get; set; }
    public decimal? PorcentajeMaximoDescuento { get; set; }
    public bool EsRpt { get; set; }
    public bool EsSistema { get; set; }
    public bool EsPack { get; set; }
    public int CantidadComponentes { get; set; }
    public decimal PrecioVentaCalculado { get; set; }
    public bool PuedeEditar { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
