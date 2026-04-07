namespace ZuluIA_Back.Application.Features.Facturacion.DTOs;

public class OrdenCargaDto
{
    public long Id { get; set; }
    public long CartaPorteId { get; set; }
    public long? TransportistaId { get; set; }
    public string? TransportistaRazonSocial { get; set; }
    public DateOnly FechaCarga { get; set; }
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public string? Patente { get; set; }
    public bool Confirmada { get; set; }
    public string? Observacion { get; set; }
}
