namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

public class TerceroPerfilComercialDto
{
    public long TerceroId { get; set; }
    public long? ZonaComercialId { get; set; }
    public string? ZonaComercialDescripcion { get; set; }
    public string? Rubro { get; set; }
    public string? Subrubro { get; set; }
    public string? Sector { get; set; }
    public string? CondicionCobranza { get; set; }
    public string RiesgoCrediticio { get; set; } = "NORMAL";
    public decimal? SaldoMaximoVigente { get; set; }
    public string? VigenciaSaldo { get; set; }
    public string? CondicionVenta { get; set; }
    public string? PlazoCobro { get; set; }
    public string? FacturadorPorDefecto { get; set; }
    public decimal? MinimoFacturaMipymes { get; set; }
    public string? ObservacionComercial { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
