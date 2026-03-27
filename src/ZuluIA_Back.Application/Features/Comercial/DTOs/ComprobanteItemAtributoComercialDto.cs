namespace ZuluIA_Back.Application.Features.Comercial.DTOs;

public class ComprobanteItemAtributoComercialDto
{
    public long Id { get; init; }
    public long ComprobanteItemId { get; init; }
    public long AtributoComercialId { get; init; }
    public string AtributoCodigo { get; init; } = string.Empty;
    public string AtributoDescripcion { get; init; } = string.Empty;
    public string TipoDato { get; init; } = string.Empty;
    public string Valor { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
