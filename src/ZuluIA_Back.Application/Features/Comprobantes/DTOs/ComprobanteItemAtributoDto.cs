namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ComprobanteItemAtributoDto
{
    public long Id { get; set; }
    public long AtributoComercialId { get; set; }
    public string AtributoCodigo { get; set; } = string.Empty;
    public string AtributoDescripcion { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
}
