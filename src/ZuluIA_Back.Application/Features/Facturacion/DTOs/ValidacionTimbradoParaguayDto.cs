namespace ZuluIA_Back.Application.Features.Facturacion.DTOs;

public class ValidacionTimbradoParaguayDto
{
    public long SucursalId { get; set; }
    public long? PuntoFacturacionId { get; set; }
    public long TipoComprobanteId { get; set; }
    public DateOnly Fecha { get; set; }
    public long NumeroComprobante { get; set; }
    public bool EsSucursalParaguay { get; set; }
    public bool RequiereTimbrado { get; set; }
    public bool EsValido { get; set; }
    public string? Mensaje { get; set; }
    public long? TimbradoId { get; set; }
    public string? NroTimbrado { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public int? NroComprobanteDesde { get; set; }
    public int? NroComprobanteHasta { get; set; }
}