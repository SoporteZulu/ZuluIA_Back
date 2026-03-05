namespace ZuluIA_Back.Application.Features.Finanzas.DTOs;

public class CobroDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Cotizacion { get; set; }
    public decimal Total { get; set; }
    public string? Observacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int? NroCierre { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IReadOnlyList<CobroMedioDto> Medios { get; set; } = [];
}

public class CobroMedioDto
{
    public long Id { get; set; }
    public long CajaId { get; set; }
    public string CajaDescripcion { get; set; } = string.Empty;
    public long FormaPagoId { get; set; }
    public string FormaPagoDescripcion { get; set; } = string.Empty;
    public long? ChequeId { get; set; }
    public decimal Importe { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Cotizacion { get; set; }
}