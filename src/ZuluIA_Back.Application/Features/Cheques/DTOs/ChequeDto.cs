namespace ZuluIA_Back.Application.Features.Cheques.DTOs;

public class ChequeDto
{
    public long Id { get; set; }
    public long CajaId { get; set; }
    public long? TerceroId { get; set; }
    public string NroCheque { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public DateOnly FechaEmision { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public DateOnly? FechaAcreditacion { get; set; }
    public DateOnly? FechaDeposito { get; set; }
    public decimal Importe { get; set; }
    public long MonedaId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}