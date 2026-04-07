namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ComprobanteCotDto
{
    public string Numero { get; set; } = string.Empty;
    public DateOnly FechaVigencia { get; set; }
    public string? Descripcion { get; set; }
}

public class ComprobanteAtributoDto
{
    public long Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string? Valor { get; set; }
    public string? TipoDato { get; set; }
}
