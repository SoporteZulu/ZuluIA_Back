namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

public class TerceroMedioContactoDto
{
    public long Id { get; set; }
    public long TerceroId { get; set; }
    public long? TipoMedioContactoId { get; set; }
    public string Valor { get; set; } = string.Empty;
    public string TipoInferidoCodigo { get; set; } = "OTRO";
    public string TipoInferidoDescripcion { get; set; } = "Otro";
    public string? Observacion { get; set; }
    public int Orden { get; set; }
    public bool EsDefecto { get; set; }
}
