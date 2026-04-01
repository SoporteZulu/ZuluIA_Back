namespace ZuluIA_Back.Application.Features.Items.DTOs;

public class ItemPackComponenteDto
{
    public long Id { get; set; }
    public long ItemPadreId { get; set; }
    public long ComponenteId { get; set; }
    public string ComponenteCodigo { get; set; } = string.Empty;
    public string ComponenteDescripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public long? UnidadMedidaId { get; set; }
    public string? UnidadMedidaDescripcion { get; set; }
}
