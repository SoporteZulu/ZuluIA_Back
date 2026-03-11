namespace ZuluIA_Back.Application.Features.Cajas.DTOs;

public class CajaDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long TipoId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public long MonedaId { get; set; }
    public string? Banco { get; set; }
    public string? NroCuenta { get; set; }
    public string? Cbu { get; set; }
    public long? UsuarioId { get; set; }
    public int NroCierreActual { get; set; }
    public bool EsCaja { get; set; }
    public bool Activa { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}