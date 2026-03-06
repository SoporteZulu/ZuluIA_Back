namespace ZuluIA_Back.Application.Features.Contabilidad.DTOs;

public class AsientoListDto
{
    public long Id { get; set; }
    public long EjercicioId { get; set; }
    public long SucursalId { get; set; }
    public DateOnly Fecha { get; set; }
    public long Numero { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public string? OrigenTabla { get; set; }
    public long? OrigenId { get; set; }
}