namespace ZuluIA_Back.Application.Features.Cobros.DTOs;

public class CobroMedioDto
{
    public long Id { get; set; }
    public long CajaId { get; set; }
    public long FormaPagoId { get; set; }
    public long? ChequeId { get; set; }
    public decimal Importe { get; set; }
    public long MonedaId { get; set; }
    public decimal Cotizacion { get; set; }
}