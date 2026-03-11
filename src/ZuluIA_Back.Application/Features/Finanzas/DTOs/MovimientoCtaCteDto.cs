namespace ZuluIA_Back.Application.Features.Finanzas.DTOs;

public class MovimientoCtaCteDto
{
    public long Id { get; set; }
    public long TerceroId { get; set; }
    public long? SucursalId { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public long? ComprobanteId { get; set; }
    public string? NumeroComprobante { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public decimal Saldo { get; set; }
    public string? Descripcion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}