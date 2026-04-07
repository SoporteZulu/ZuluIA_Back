namespace ZuluIA_Back.Application.Features.Comercial.DTOs;

public class CatalogoComercialDto
{
    public long Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public bool Activo { get; init; }
}

public class MaestroAuxiliarComercialDto : CatalogoComercialDto
{
    public string Grupo { get; init; } = string.Empty;
}

public class AtributoComercialDto : CatalogoComercialDto
{
    public string TipoDato { get; init; } = string.Empty;
}
