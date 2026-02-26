namespace ZuluIA_Back.Application.Features.Contabilidad.DTOs;

public class AsientoLineaDto
{
    public long Id { get; set; }
    public long CuentaId { get; set; }
    public string? CuentaCodigo { get; set; }
    public string? CuentaNombre { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string? Descripcion { get; set; }
    public short Orden { get; set; }
    public long? CentroCostoId { get; set; }
}