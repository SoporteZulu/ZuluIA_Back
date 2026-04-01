namespace ZuluIA_Back.Application.Features.ListasPrecios.DTOs;

public class ListaPreciosDto
{
    public long Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public long MonedaId { get; set; }
    public string? MonedaDescripcion { get; set; }
    public string? MonedaSimbolo { get; set; }
    public DateOnly? VigenciaDesde { get; set; }
    public DateOnly? VigenciaHasta { get; set; }
    public bool Activa { get; set; }
    public bool EsPorDefecto { get; set; }
    public long? ListaPadreId { get; set; }
    public string? ListaPadreDescripcion { get; set; }
    public int Prioridad { get; set; }
    public string? Observaciones { get; set; }
    public bool EstaVigenteHoy { get; set; }
    public bool TieneHerencia { get; set; }
    public int CantidadItems { get; set; }
    public int CantidadPersonasAsignadas { get; set; }
    public int CantidadPromocionesActivas { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}