namespace ZuluIA_Back.Application.Features.Cotizaciones.DTOs;

public class CotizacionMonedaDto
{
    public long Id { get; set; }
    public long MonedaId { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal Cotizacion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}