namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

public class TerceroListDto
{
    public long Id { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }
    public string NroDocumento { get; set; } = string.Empty;
    public bool EsCliente { get; set; }
    public bool EsProveedor { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; }
}