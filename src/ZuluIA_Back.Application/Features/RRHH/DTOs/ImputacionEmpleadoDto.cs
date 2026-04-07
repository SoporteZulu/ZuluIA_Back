namespace ZuluIA_Back.Application.Features.RRHH.DTOs;

public class ImputacionEmpleadoDto
{
    public long Id { get; set; }
    public long LiquidacionSueldoId { get; set; }
    public long? ComprobanteEmpleadoId { get; set; }
    public long? TesoreriaMovimientoId { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal Importe { get; set; }
    public string? Observacion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
