namespace ZuluIA_Back.Application.Features.Facturacion.DTOs;

public class CartaPorteEventoDto
{
    public long Id { get; set; }
    public long CartaPorteId { get; set; }
    public long? OrdenCargaId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string? EstadoAnterior { get; set; }
    public string EstadoNuevo { get; set; } = string.Empty;
    public DateOnly FechaEvento { get; set; }
    public string? Mensaje { get; set; }
    public string? NroCtg { get; set; }
    public int IntentoCtg { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
}
