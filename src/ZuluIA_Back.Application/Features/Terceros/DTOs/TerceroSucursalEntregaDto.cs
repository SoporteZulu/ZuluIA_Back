namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

public class TerceroSucursalEntregaDto
{
    public long Id { get; set; }
    public long TerceroId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Localidad { get; set; }
    public string? Responsable { get; set; }
    public string? Telefono { get; set; }
    public string? Horario { get; set; }
    public bool Principal { get; set; }
    public int Orden { get; set; }
}
