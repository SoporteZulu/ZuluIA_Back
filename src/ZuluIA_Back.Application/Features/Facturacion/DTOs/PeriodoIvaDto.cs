namespace ZuluIA_Back.Application.Features.Facturacion.DTOs;

public class PeriodoIvaDto
{
    public long Id { get; set; }
    public long EjercicioId { get; set; }
    public long SucursalId { get; set; }
    public DateOnly Periodo { get; set; }
    public string PeriodoDescripcion { get; set; } = string.Empty;
    public bool Cerrado { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}