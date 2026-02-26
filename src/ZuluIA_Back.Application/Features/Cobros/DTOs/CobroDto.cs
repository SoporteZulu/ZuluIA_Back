using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Cobros.DTOs;

public class CobroDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long TerceroId { get; set; }
    public DateOnly Fecha { get; set; }
    public long MonedaId { get; set; }
    public decimal Cotizacion { get; set; }
    public decimal Total { get; set; }
    public string? Observacion { get; set; }
    public EstadoCobro Estado { get; set; }
    public int? NroCierre { get; set; }
    public List<CobroMedioDto> Medios { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
}