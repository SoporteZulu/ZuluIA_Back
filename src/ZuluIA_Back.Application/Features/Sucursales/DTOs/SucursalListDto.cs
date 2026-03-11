namespace ZuluIA_Back.Application.Features.Sucursales.DTOs;

public class SucursalListDto
{
    public long Id { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }
    public string Cuit { get; set; } = string.Empty;
    public bool CasaMatriz { get; set; }
    public bool Activa { get; set; }
}