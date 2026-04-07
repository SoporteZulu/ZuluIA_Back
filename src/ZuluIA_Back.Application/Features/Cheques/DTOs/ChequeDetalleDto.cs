namespace ZuluIA_Back.Application.Features.Cheques.DTOs;

public class ChequeDetalleDto
{
    public long Id { get; set; }
    
    // Información básica
    public long CajaId { get; set; }
    public string CajaDescripcion { get; set; } = string.Empty;
    public long? TerceroId { get; set; }
    public string? TerceroRazonSocial { get; set; }
    public string? TerceroNroDocumento { get; set; }
    
    // Datos del cheque
    public string NroCheque { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public string? CodigoSucursalBancaria { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Titular { get; set; }
    
    // Fechas
    public DateOnly FechaEmision { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public DateOnly? FechaAcreditacion { get; set; }
    public DateOnly? FechaDeposito { get; set; }
    
    // Importe y moneda
    public decimal Importe { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = "$";
    public string MonedaDescripcion { get; set; } = string.Empty;
    
    // Estado y tipo
    public string Estado { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    
    // Características
    public bool EsALaOrden { get; set; }
    public bool EsCruzado { get; set; }
    
    // Referencias
    public long? ChequeraId { get; set; }
    public string? ChequeraDescripcion { get; set; }
    public string? ChequeraBancoDescripcion { get; set; }
    public string? ChequeraNroCuenta { get; set; }
    
    public long? ComprobanteOrigenId { get; set; }
    public string? ComprobanteOrigenNumero { get; set; }
    public string? ComprobanteOrigenTipo { get; set; }
    public DateOnly? ComprobanteOrigenFecha { get; set; }
    
    // Observaciones
    public string? Observacion { get; set; }
    public string? ConceptoRechazo { get; set; }
    
    // Auditoría
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public string? CreatedByUsuario { get; set; }
    public long? UpdatedBy { get; set; }
    public string? UpdatedByUsuario { get; set; }
    
    // Historial de movimientos
    public IReadOnlyList<ChequeHistorialDto> Historial { get; set; } = [];
    
    // Indicadores calculados
    public bool EstaVencido => FechaVencimiento < DateOnly.FromDateTime(DateTime.Today);
    public int DiasHastaVencimiento => (FechaVencimiento.ToDateTime(TimeOnly.MinValue) 
                                       - DateTime.Today).Days;
    public bool PuedeDepositar => Estado == "CARTERA";
    public bool PuedeAcreditar => Estado == "DEPOSITADO";
    public bool PuedeRechazar => Estado is "CARTERA" or "DEPOSITADO" or "ENTRANSITO";
    public bool PuedeEndosar => Estado is "CARTERA" or "ENDOSADO" && EsALaOrden;
    public bool PuedeAnular => Estado == "CARTERA" && Tipo == "PROPIO";
    public bool PuedeEditar => Estado == "CARTERA";
}
