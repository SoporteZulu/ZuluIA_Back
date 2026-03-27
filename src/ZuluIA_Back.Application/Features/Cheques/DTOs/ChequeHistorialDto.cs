namespace ZuluIA_Back.Application.Features.Cheques.DTOs;

public class ChequeHistorialDto
{
    public long Id { get; set; }
    public long ChequeId { get; set; }
    public long CajaId { get; set; }
    public string CajaDescripcion { get; set; } = string.Empty;
    public long? TerceroId { get; set; }
    public string? TerceroRazonSocial { get; set; }
    public string Operacion { get; set; } = string.Empty;
    public string? EstadoAnterior { get; set; }
    public string EstadoNuevo { get; set; } = string.Empty;
    public DateOnly FechaOperacion { get; set; }
    public DateOnly? FechaAcreditacion { get; set; }
    public string? Observacion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
}
