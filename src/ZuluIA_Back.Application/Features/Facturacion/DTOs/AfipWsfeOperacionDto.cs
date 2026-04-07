namespace ZuluIA_Back.Application.Features.Facturacion.DTOs;

public class AfipWsfeOperacionDto
{
    public long ComprobanteId { get; set; }
    public string Operacion { get; set; } = string.Empty;
    public bool Exitoso { get; set; }
    public string EstadoAfip { get; set; } = string.Empty;
    public string? CodigoError { get; set; }
    public string? Cae { get; set; }
    public string? Caea { get; set; }
    public DateOnly? FechaVto { get; set; }
    public string? QrData { get; set; }
    public string? Mensaje { get; set; }
}
