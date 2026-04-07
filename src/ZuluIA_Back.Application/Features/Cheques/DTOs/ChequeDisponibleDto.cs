namespace ZuluIA_Back.Application.Features.Cheques.DTOs;

/// <summary>
/// DTO de cheque de tercero disponible para selección en cobros/pagos
/// </summary>
public class ChequeDisponibleDto
{
    public long Id { get; set; }
    public string NumeroCheque { get; set; } = string.Empty;
    public long BancoId { get; set; }
    public string BancoDescripcion { get; set; } = string.Empty;
    public string? SucursalBancaria { get; set; }
    public string Plaza { get; set; } = string.Empty;
    public string Cuit { get; set; } = string.Empty;
    public string NombreTitular { get; set; } = string.Empty;
    public DateOnly FechaEmision { get; set; }
    public DateOnly FechaPago { get; set; }
    public int DiasAlCobro { get; set; }
    public bool EsDiferido { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public string EstadoCheque { get; set; } = string.Empty;
    
    // Origen del cheque
    public long ClienteOrigenId { get; set; }
    public string ClienteOrigenRazonSocial { get; set; } = string.Empty;
    public long? CobroOrigenId { get; set; }
    public DateOnly FechaRecepcion { get; set; }
    
    public string? Observaciones { get; set; }
}
