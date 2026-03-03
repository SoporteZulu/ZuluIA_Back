namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

/// <summary>
/// DTO de detalle completo de un tercero.
/// Usado en GET /api/terceros/{id} y como respuesta de Create/Update.
/// Equivalente a la pantalla de ABM de Clientes/Proveedores del VB6
/// con todos sus campos visibles.
/// </summary>
public class TerceroDto
{
    // ─── Identificación ───────────────────────────────────────────────────────
    public long Id { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }

    // ─── Documento e IVA ──────────────────────────────────────────────────────
    public long TipoDocumentoId { get; set; }
    public string TipoDocumentoDescripcion { get; set; } = string.Empty;
    public string NroDocumento { get; set; } = string.Empty;
    public long CondicionIvaId { get; set; }
    public string CondicionIvaDescripcion { get; set; } = string.Empty;

    // ─── Clasificación / Roles ────────────────────────────────────────────────
    public long? CategoriaId { get; set; }
    public string? CategoriaDescripcion { get; set; }
    public bool EsCliente { get; set; }
    public bool EsProveedor { get; set; }
    public bool EsEmpleado { get; set; }

    // ─── Domicilio ────────────────────────────────────────────────────────────
    public DomicilioDto Domicilio { get; set; } = new();

    // ─── Domicilio aplanado (opcional, útil para mapeos directos) ─────────────
    public string? Calle { get; set; }
    public string? Nro { get; set; }
    public string? Piso { get; set; }
    public string? Dpto { get; set; }
    public string? CodigoPostal { get; set; }
    public long? LocalidadId { get; set; }
    public string? LocalidadDescripcion { get; set; }
    public long? BarrioId { get; set; }
    public string? BarrioDescripcion { get; set; }
    public string Completo { get; set; } = string.Empty;

    // ─── Datos fiscales ───────────────────────────────────────────────────────
    public string? NroIngresosBrutos { get; set; }
    public string? NroMunicipal { get; set; }

    // ─── Contacto ─────────────────────────────────────────────────────────────
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }
    public string? Web { get; set; }

    // ─── Comercial ────────────────────────────────────────────────────────────
    public long? MonedaId { get; set; }
    public string? MonedaDescripcion { get; set; }
    public decimal? LimiteCredito { get; set; }
    public bool Facturable { get; set; }
    public long? CobradorId { get; set; }
    public string? CobradorNombre { get; set; }
    public decimal PctComisionCobrador { get; set; }
    public long? VendedorId { get; set; }
    public string? VendedorNombre { get; set; }
    public decimal PctComisionVendedor { get; set; }
    public string? Observacion { get; set; }

    // ─── Control ──────────────────────────────────────────────────────────────
    public long? SucursalId { get; set; }
    public string? SucursalDescripcion { get; set; }
    public bool Activo { get; set; }

    // ─── Auditoría ────────────────────────────────────────────────────────────
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>
/// Sub-DTO del domicilio, alineado con el ValueObject Domicilio.
/// Se devuelve anidado dentro de TerceroDto para que el frontend
/// pueda armar la pantalla de domicilio en un solo bloque.
/// </summary>
public class DomicilioDto
{
    public string? Calle { get; set; }
    public string? Nro { get; set; }
    public string? Piso { get; set; }
    public string? Dpto { get; set; }
    public string? CodigoPostal { get; set; }
    public long? LocalidadId { get; set; }
    public string? LocalidadDescripcion { get; set; }
    public long? BarrioId { get; set; }
    public string? BarrioDescripcion { get; set; }
    public string Completo { get; set; } = string.Empty;
}
