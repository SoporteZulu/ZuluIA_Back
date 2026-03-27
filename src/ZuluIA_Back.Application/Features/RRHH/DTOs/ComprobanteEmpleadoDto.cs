namespace ZuluIA_Back.Application.Features.RRHH.DTOs;

public class ComprobanteEmpleadoDto
{
    public long Id { get; set; }
    public long EmpleadoId { get; set; }
    public long LiquidacionSueldoId { get; set; }
    public long SucursalId { get; set; }
    public DateOnly Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public string? Observacion { get; set; }
}
