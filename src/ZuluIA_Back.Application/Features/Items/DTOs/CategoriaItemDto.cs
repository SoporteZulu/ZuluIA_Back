namespace ZuluIA_Back.Application.Features.Items.DTOs;

public class CategoriaItemDto
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public short Nivel { get; set; }
    public string? OrdenNivel { get; set; }
    public bool Activo { get; set; }
    public IReadOnlyList<CategoriaItemDto> Hijos { get; set; } = [];
}