namespace ZuluIA_Back.Application.Features.Contabilidad.DTOs;

public class CentroCostoDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
}