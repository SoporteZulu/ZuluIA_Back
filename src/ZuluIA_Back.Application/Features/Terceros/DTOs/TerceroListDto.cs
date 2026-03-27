namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

/// <summary>
/// DTO de fila para el listado paginado de terceros.
/// Usado en GET /api/terceros (paginado).
/// Equivalente a las columnas de la grilla del ABM de Clientes/Proveedores del VB6:
/// Legajo | Razón Social | CUIT | Condición IVA | Tel | Email | Roles | Estado
/// </summary>
public class TerceroListDto
{
    // ─── Identificación ───────────────────────────────────────────────────────
    public long Id { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }
    public string TipoPersoneria { get; set; } = "JURIDICA";

    // ─── Documento e IVA ──────────────────────────────────────────────────────
    public long CondicionIvaId { get; set; }
    public string NroDocumento { get; set; } = string.Empty;
    public string CondicionIvaDescripcion { get; set; } = string.Empty;

    // ─── Roles ────────────────────────────────────────────────────────────────
    public bool EsCliente { get; set; }
    public bool EsProveedor { get; set; }
    public bool EsEmpleado { get; set; }

    // ─── Contacto rápido ──────────────────────────────────────────────────────
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }
    public string? Web { get; set; }

    // ─── Domicilio ────────────────────────────────────────────────────────────
    public string? Calle { get; set; }
    public string? Nro { get; set; }
    public string? Piso { get; set; }
    public string? Dpto { get; set; }
    public string? CodigoPostal { get; set; }
    public long? LocalidadId { get; set; }
    public long? BarrioId { get; set; }
    public string? NroIngresosBrutos { get; set; }
    public string? NroMunicipal { get; set; }

    // ─── Localidad (para columna de ubicación en grilla) ─────────────────────
    public string? LocalidadDescripcion { get; set; }

    // ─── Comercial (para grilla de clientes con límite de crédito) ───────────
    public long? MonedaId { get; set; }
    public long? CategoriaId { get; set; }
    public decimal? LimiteCredito { get; set; }
    public bool Facturable { get; set; }
    public long? CobradorId { get; set; }
    public decimal PctComisionCobrador { get; set; }
    public long? VendedorId { get; set; }
    public decimal PctComisionVendedor { get; set; }
    public string? Observacion { get; set; }

    // ─── Control ──────────────────────────────────────────────────────────────
    public bool Activo { get; set; }
    public string RolDisplay { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}