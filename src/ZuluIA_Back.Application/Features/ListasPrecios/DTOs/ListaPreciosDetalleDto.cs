namespace ZuluIA_Back.Application.Features.ListasPrecios.DTOs;

/// <summary>
/// Detalle completo de una lista de precios incluyendo todos sus ítems.
/// </summary>
public class ListaPreciosDetalleDto
{
    public long Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public long MonedaId { get; set; }
    public DateOnly? VigenciaDesde { get; set; }
    public DateOnly? VigenciaHasta { get; set; }
    public bool Activa { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public IReadOnlyList<ListaPreciosItemDto> Items { get; set; } = [];
}