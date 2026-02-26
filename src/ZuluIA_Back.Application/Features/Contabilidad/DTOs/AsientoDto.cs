using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Contabilidad.DTOs;

public class AsientoDto
{
    public long Id { get; set; }
    public long EjercicioId { get; set; }
    public long SucursalId { get; set; }
    public DateOnly Fecha { get; set; }
    public long Numero { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? OrigenTabla { get; set; }
    public long? OrigenId { get; set; }
    public EstadoAsiento Estado { get; set; }
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public List<AsientoLineaDto> Lineas { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
}