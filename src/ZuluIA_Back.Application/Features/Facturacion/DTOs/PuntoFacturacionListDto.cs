namespace ZuluIA_Back.Application.Features.Facturacion.DTOs;

public class PuntoFacturacionListDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public short Numero { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}