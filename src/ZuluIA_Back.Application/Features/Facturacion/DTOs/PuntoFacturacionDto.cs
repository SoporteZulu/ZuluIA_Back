namespace ZuluIA_Back.Application.Features.Facturacion.DTOs;

public class PuntoFacturacionDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long TipoId { get; set; }
    public short Numero { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}