namespace ZuluIA_Back.Application.Features.Configuracion.DTOs;

public class ConfiguracionDto
{
    public long Id { get; set; }
    public string Campo { get; set; } = string.Empty;
    public string? Valor { get; set; }
    public short TipoDato { get; set; }
    public string? Descripcion { get; set; }
}