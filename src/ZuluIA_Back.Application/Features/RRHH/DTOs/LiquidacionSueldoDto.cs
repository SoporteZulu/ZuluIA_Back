namespace ZuluIA_Back.Application.Features.RRHH.DTOs;

public class LiquidacionSueldoDto
{
    public long Id { get; set; }
    public long EmpleadoId { get; set; }
    public string EmpleadoLegajo { get; set; } = string.Empty;
    public string EmpleadoNombre { get; set; } = string.Empty;
    public long SucursalId { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public decimal SueldoBasico { get; set; }
    public decimal TotalHaberes { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal Neto { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public bool Pagada { get; set; }
    public decimal ImporteImputado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateOnly? FechaPago { get; set; }
    public long? ComprobanteEmpleadoId { get; set; }
    public string? Observacion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}