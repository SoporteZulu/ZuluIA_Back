namespace ZuluIA_Back.Application.Features.Cajas.DTOs;

public class CajaListDto
{
    public long Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public long MonedaId { get; set; }
    public bool EsCaja { get; set; }
    public bool Activa { get; set; }
    public long SucursalId { get; set; }
    public long? UsuarioId { get; set; }
    public int NroCierreActual { get; set; }
}