namespace ZuluIA_Back.Application.Features.Finanzas.DTOs;

public class CuentaCorrienteDto
{
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public long? SucursalId { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}