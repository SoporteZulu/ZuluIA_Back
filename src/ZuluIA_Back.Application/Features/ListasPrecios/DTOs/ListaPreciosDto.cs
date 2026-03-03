namespace ZuluIA_Back.Application.Features.ListasPrecios.DTOs;

public class ListaPreciosDto
{
    public long Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public long MonedaId { get; set; }
    public DateOnly? VigenciaDesde { get; set; }
    public DateOnly? VigenciaHasta { get; set; }
    public bool Activa { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}