namespace ZuluIA_Back.Application.Features.Facturacion.DTOs;

public class AfipWsfeConfiguracionDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long PuntoFacturacionId { get; set; }
    public bool Habilitado { get; set; }
    public bool Produccion { get; set; }
    public bool UsaCaeaPorDefecto { get; set; }
    public string CuitEmisor { get; set; } = string.Empty;
    public string? CertificadoAlias { get; set; }
    public string? Observacion { get; set; }
}
