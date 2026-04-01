namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

using ZuluIA_Back.Domain.Enums;

public class ComprobanteDetalleDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public string SucursalRazonSocial { get; set; } = string.Empty;
    public long? PuntoFacturacionId { get; set; }
    public long TipoComprobanteId { get; set; }
    public string TipoComprobanteDescripcion { get; set; } = string.Empty;
    public string? TipoComprobanteCodigo { get; set; }
    public short Prefijo { get; set; }
    public long Numero { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public string TerceroCuit { get; set; } = string.Empty;
    public string TerceroCondicionIva { get; set; } = string.Empty;
    public string? TerceroLegajo { get; set; }
    public string? TerceroDomicilioSnapshot { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Cotizacion { get; set; }
    
    // Datos comerciales
    public long? VendedorId { get; set; }
    public string? VendedorNombre { get; set; }
    public string? VendedorLegajo { get; set; }
    public long? CobradorId { get; set; }
    public string? CobradorNombre { get; set; }
    public string? CobradorLegajo { get; set; }
    public long? ZonaComercialId { get; set; }
    public string? ZonaComercialDescripcion { get; set; }
    public long? ListaPreciosId { get; set; }
    public string? ListaPreciosDescripcion { get; set; }
    public long? CondicionPagoId { get; set; }
    public string? CondicionPagoDescripcion { get; set; }
    public int? PlazoDias { get; set; }
    public long? CanalVentaId { get; set; }
    public string? CanalVentaDescripcion { get; set; }
    public decimal? PorcentajeComisionVendedor { get; set; }
    public decimal? PorcentajeComisionCobrador { get; set; }
    public decimal? ImporteComisionVendedor { get; set; }
    public decimal? ImporteComisionCobrador { get; set; }
    public long? MotivoDebitoId { get; set; }
    public string? MotivoDebitoDescripcion { get; set; }
    public string? MotivoDebitoObservacion { get; set; }
    public bool? MotivoDebitoEsFiscal { get; set; }
    
    // Datos logísticos
    public long? TransporteId { get; set; }
    public string? TransporteRazonSocial { get; set; }
    public string? ChoferNombre { get; set; }
    public string? ChoferDni { get; set; }
    public string? PatVehiculo { get; set; }
    public string? PatAcoplado { get; set; }
    public string? RutaLogistica { get; set; }
    public string? DomicilioEntrega { get; set; }
    public string? ObservacionesLogisticas { get; set; }
    public DateOnly? FechaEstimadaEntrega { get; set; }
    public DateOnly? FechaRealEntrega { get; set; }
    public string? FirmaConformidad { get; set; }
    public string? NombreQuienRecibe { get; set; }
    public string? DniQuienRecibe { get; set; }
    public EstadoLogisticoRemito? EstadoLogistico { get; set; }
    public bool EsValorizado { get; set; }
    public long? DepositoOrigenId { get; set; }
    public string? DepositoOrigenDescripcion { get; set; }
    public string? CotNumero { get; set; }
    public DateOnly? CotFechaVigencia { get; set; }
    public string? CotDescripcion { get; set; }
    public ComprobanteCotDto? Cot { get; set; }
    public decimal? PesoTotal { get; set; }
    public decimal? VolumenTotal { get; set; }
    public int? Bultos { get; set; }
    public string? TipoEmbalaje { get; set; }
    public bool? SeguroTransporte { get; set; }
    public decimal? ValorDeclarado { get; set; }
    
    // Observaciones extendidas
    public string? ObservacionInterna { get; set; }
    public string? ObservacionFiscal { get; set; }
    
    // Recargo y descuento global
    public decimal? RecargoPorcentaje { get; set; }
    public decimal? RecargoImporte { get; set; }
    public decimal? DescuentoPorcentaje { get; set; }
    
    public decimal Subtotal { get; set; }
    public decimal DescuentoImporte { get; set; }
    public decimal NetoGravado { get; set; }
    public decimal NetoNoGravado { get; set; }
    public decimal IvaRi { get; set; }
    public decimal IvaRni { get; set; }
    public decimal Percepciones { get; set; }
    public decimal Retenciones { get; set; }
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public long? TimbradoId { get; set; }
    public string? NroTimbrado { get; set; }
    public EstadoSifenParaguay? EstadoSifen { get; set; }
    public string? SifenCodigoRespuesta { get; set; }
    public string? SifenMensajeRespuesta { get; set; }
    public string? SifenTrackingId { get; set; }
    public string? SifenCdc { get; set; }
    public string? SifenNumeroLote { get; set; }
    public DateTimeOffset? SifenFechaRespuesta { get; set; }
    public bool TieneIdentificadoresSifen { get; set; }
    public bool PuedeReintentarSifen { get; set; }
    public bool PuedeConciliarSifen { get; set; }
    public string? Cae { get; set; }
    public string? Caea { get; set; }
    public DateOnly? FechaVtoCae { get; set; }
    public string? QrData { get; set; }
    public string EstadoAfip { get; set; } = string.Empty;
    public string? UltimoErrorAfip { get; set; }
    public DateTimeOffset? FechaUltimaConsultaAfip { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    // Auditoría de usuarios
    public long? CreatedBy { get; set; }
    public string? CreatedByUsuario { get; set; }
    public long? UpdatedBy { get; set; }
    public string? UpdatedByUsuario { get; set; }
    public DateTimeOffset? FechaAnulacion { get; set; }
    public long? UsuarioAnulacionId { get; set; }
    public string? UsuarioAnulacionNombre { get; set; }
    public string? MotivoAnulacion { get; set; }
    
    // Comprobante origen con detalle
    public long? ComprobanteOrigenId { get; set; }
    public string? ComprobanteOrigenNumero { get; set; }
    public string? ComprobanteOrigenTipo { get; set; }
    public DateOnly? ComprobanteOrigenFecha { get; set; }
    
    // Metadatos de devolución (para notas de crédito/débito de devolución)
    public MotivoDevolucion? MotivoDevolucion { get; set; }
    public string? MotivoDevolucionDescripcion { get; set; }
    public TipoDevolucion? TipoDevolucion { get; set; }
    public string? TipoDevolucionDescripcion { get; set; }
    public long? AutorizadorDevolucionId { get; set; }
    public string? AutorizadorDevolucionNombre { get; set; }
    public DateTimeOffset? FechaAutorizacionDevolucion { get; set; }
    public string? ObservacionDevolucion { get; set; }
    public bool ReingresaStock { get; set; }
    public bool AcreditaCuentaCorriente { get; set; }
    
    // Metadatos de pedido (para notas de pedido)
    public EstadoPedido? EstadoPedido { get; set; }
    public string? EstadoPedidoDescripcion { get; set; }
    public DateOnly? FechaEntregaCompromiso { get; set; }
    public bool TienePedidosAtrasados { get; set; }
    public decimal PorcentajeCumplimiento { get; set; }
    public IReadOnlyList<ComprobanteAtributoDto> AtributosRemito { get; set; } = [];
    
    public IReadOnlyList<ComprobanteItemDto> Items { get; set; } = [];
    public IReadOnlyList<ImputacionDto> Imputaciones { get; set; } = [];
}