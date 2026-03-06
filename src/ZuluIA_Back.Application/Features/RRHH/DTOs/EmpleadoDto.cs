namespace ZuluIA_Back.Application.Features.RRHH.DTOs;

public class EmpleadoDto
{
    public long Id { get; set; }
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public string TerceroCuit { get; set; } = string.Empty;
    public long SucursalId { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string? Area { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public DateOnly? FechaEgreso { get; set; }
    public decimal SueldoBasico { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}