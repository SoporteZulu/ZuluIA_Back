namespace ZuluIA_Back.Application.Features.Usuarios.DTOs;

public class MenuItemDto
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Formulario { get; set; }
    public string? Icono { get; set; }
    public short Nivel { get; set; }
    public short Orden { get; set; }
    public bool Activo { get; set; }
    public IReadOnlyList<MenuItemDto> Hijos { get; set; } = [];
}