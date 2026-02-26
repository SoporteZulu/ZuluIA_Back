namespace ZuluIA_Back.Application.Features.Pagos.DTOs;

public class PagoMedioDto
{
    public long Id { get; set; }
    public long CajaId { get; set; }
    public long FormaPagoId { get; set; }
    public long? ChequeId { get; set; }
    public decimal Importe { get; set; }
    public long MonedaId { get; set; }
    public decimal Cotizacion { get; set; }
}