namespace ZuluIA_Back.Application.Features.Cheques.DTOs;

public class ChequeDto
{
    public long Id { get; set; }
    public long CajaId { get; set; }
    public string? CajaDescripcion { get; set; }
    public long? TerceroId { get; set; }
    public string? TerceroRazonSocial { get; set; }
    public string NroCheque { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public string? CodigoSucursalBancaria { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Titular { get; set; }
    public DateOnly FechaEmision { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public DateOnly? FechaAcreditacion { get; set; }
    public DateOnly? FechaDeposito { get; set; }
    public decimal Importe { get; set; }
    public long MonedaId { get; set; }
    public string? MonedaSimbolo { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool EsALaOrden { get; set; }
    public bool EsCruzado { get; set; }
    public long? ChequeraId { get; set; }
    public string? ChequeraDescripcion { get; set; }
    public long? ComprobanteOrigenId { get; set; }
    public string? ComprobanteOrigenNumero { get; set; }
    public string? Observacion { get; set; }
    public string? ConceptoRechazo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}