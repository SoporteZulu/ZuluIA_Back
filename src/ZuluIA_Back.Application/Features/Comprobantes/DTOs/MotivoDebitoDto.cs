namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class MotivoDebitoDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool EsFiscal { get; set; }
    public bool RequiereDocumentoOrigen { get; set; }
    public bool AfectaCuentaCorriente { get; set; }
    public bool Activo { get; set; }
}
