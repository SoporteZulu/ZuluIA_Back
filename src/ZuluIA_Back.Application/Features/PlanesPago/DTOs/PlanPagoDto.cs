namespace ZuluIA_Back.Application.Features.PlanesPago.DTOs;

public class PlanPagoDto
{
    public long Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public short CantidadCuotas { get; set; }
    public decimal InteresPct { get; set; }
    public bool Activo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}