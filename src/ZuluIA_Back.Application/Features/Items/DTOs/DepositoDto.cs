namespace ZuluIA_Back.Application.Features.Items.DTOs;

public class DepositoDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool EsDefault { get; set; }
    public bool Activo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}