namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

public class TerceroDto
{
    public long Id { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }
    public long TipoDocumentoId { get; set; }
    public string NroDocumento { get; set; } = string.Empty;
    public long CondicionIvaId { get; set; }
    public long? CategoriaId { get; set; }
    public bool EsCliente { get; set; }
    public bool EsProveedor { get; set; }
    public bool EsEmpleado { get; set; }
    public string? Calle { get; set; }
    public string? Nro { get; set; }
    public string? CodigoPostal { get; set; }
    public long? LocalidadId { get; set; }
    public string? NroIngresosBrutos { get; set; }
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }
    public string? Web { get; set; }
    public long? MonedaId { get; set; }
    public decimal? LimiteCredito { get; set; }
    public bool Facturable { get; set; }
    public bool Activo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}