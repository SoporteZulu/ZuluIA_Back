using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Pagos.DTOs;

public class PagoDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long TerceroId { get; set; }
    public DateOnly Fecha { get; set; }
    public long MonedaId { get; set; }
    public decimal Cotizacion { get; set; }
    public decimal Total { get; set; }
    public string? Observacion { get; set; }
    public EstadoPago Estado { get; set; }
    public List<PagoMedioDto> Medios { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
}