namespace ZuluIA_Back.Application.Features.Caea.DTOs;

public class CaeaListDto
{
    public long Id { get; set; }
    public long PuntoFacturacionId { get; set; }
    public string NroCaea { get; set; } = string.Empty;
    public DateOnly FechaDesde { get; set; }
    public DateOnly FechaHasta { get; set; }
    public DateOnly? FechaProcesoAfip { get; set; }
    public DateOnly? FechaTopeInformarAfip { get; set; }
    public string TipoComprobante { get; set; } = string.Empty;
    public int CantidadAsignada { get; set; }
    public int CantidadUsada { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
