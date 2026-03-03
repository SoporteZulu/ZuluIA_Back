namespace ZuluIA_Back.Application.Features.Sucursales.DTOs;

public class SucursalDto
{
    public long Id { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }
    public string Cuit { get; set; } = string.Empty;
    public string? NroIngresosBrutos { get; set; }
    public long CondicionIvaId { get; set; }
    public long MonedaId { get; set; }
    public long PaisId { get; set; }

    // Domicilio (owned value object aplanado)
    public string? Calle { get; set; }
    public string? Nro { get; set; }
    public string? Piso { get; set; }
    public string? Dpto { get; set; }
    public string? CodigoPostal { get; set; }
    public long? LocalidadId { get; set; }
    public long? BarrioId { get; set; }

    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Web { get; set; }
    public string? Cbu { get; set; }
    public string? AliasCbu { get; set; }
    public string? Cai { get; set; }
    public short PuertoAfip { get; set; }
    public bool CasaMatriz { get; set; }
    public bool Activa { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}