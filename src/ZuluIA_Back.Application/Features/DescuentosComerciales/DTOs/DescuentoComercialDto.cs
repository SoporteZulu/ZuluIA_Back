namespace ZuluIA_Back.Application.Features.DescuentosComerciales.DTOs;

public class DescuentoComercialDto
{
    public long Id { get; init; }
    public long TerceroId { get; init; }
    public long ItemId { get; init; }
    public DateOnly FechaDesde { get; init; }
    public DateOnly? FechaHasta { get; init; }
    public decimal Porcentaje { get; init; }
}
