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

    // ─── Documento e IVA ──────────────────────────────────────────────────────
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

    // ─── Localidad (para columna de ubicación en grilla) ─────────────────────
    public string? LocalidadDescripcion { get; set; }

    // ─── Comercial (para grilla de clientes con límite de crédito) ───────────
    public decimal? LimiteCredito { get; set; }
    public bool Facturable { get; set; }

    // ─── Control ──────────────────────────────────────────────────────────────
    public bool Activo { get; set; }
    public string RolDisplay { get; set; } = string.Empty;
}