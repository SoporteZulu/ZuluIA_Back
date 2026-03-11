namespace ZuluIA_Back.Application.Features.Finanzas.DTOs;

public class CobroListDto
{
    public long Id { get; set; }
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int? NroCierre { get; set; }
}