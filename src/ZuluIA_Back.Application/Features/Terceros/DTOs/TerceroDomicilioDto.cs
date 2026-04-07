namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

public class TerceroDomicilioDto
{
    public long Id { get; set; }
    public long TerceroId { get; set; }
    public long? TipoDomicilioId { get; set; }
    public string? TipoDomicilioDescripcion { get; set; }
    public long? ProvinciaId { get; set; }
    public string? ProvinciaDescripcion { get; set; }
    public long? LocalidadId { get; set; }
    public string? LocalidadDescripcion { get; set; }
    public string? Calle { get; set; }
    public string? Barrio { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Observacion { get; set; }
    public string GeografiaCompleta { get; set; } = string.Empty;
    public string UbicacionCompleta { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool EsDefecto { get; set; }
}
