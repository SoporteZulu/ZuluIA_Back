namespace ZuluIA_Back.Application.Features.Facturacion.DTOs;

public class AfipWsfeAuditDto
{
    public long Id { get; set; }
    public long ComprobanteId { get; set; }
    public long SucursalId { get; set; }
    public long PuntoFacturacionId { get; set; }
    public string Operacion { get; set; } = string.Empty;
    public bool Exitoso { get; set; }
    public string RequestPayload { get; set; } = string.Empty;
    public string ResponsePayload { get; set; } = string.Empty;
    public string? MensajeError { get; set; }
    public string? Cae { get; set; }
    public string? Caea { get; set; }
    public DateOnly FechaOperacion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
}
