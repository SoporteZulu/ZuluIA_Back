namespace ZuluIA_Back.Application.Features.Contabilidad.DTOs;

public class AsientoDto
{
    public long Id { get; set; }
    public long EjercicioId { get; set; }
    public string EjercicioDescripcion { get; set; } = string.Empty;
    public long SucursalId { get; set; }
    public DateOnly Fecha { get; set; }
    public long Numero { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? OrigenTabla { get; set; }
    public long? OrigenId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public bool Cuadra { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IReadOnlyList<AsientoLineaDto> Lineas { get; set; } = [];
}

public class AsientoLineaDto
{
    public long Id { get; set; }
    public long CuentaId { get; set; }
    public string CuentaCodigo { get; set; } = string.Empty;
    public string CuentaDenominacion { get; set; } = string.Empty;
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string? Descripcion { get; set; }
    public short Orden { get; set; }
    public long? CentroCostoId { get; set; }
    public string? CentroCostoDescripcion { get; set; }
}
