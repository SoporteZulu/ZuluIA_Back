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
    public long NroInterno { get; set; }
    public DateOnly FechaAlta { get; set; }
    public DateOnly? FechaRegistro { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }
    public string TipoPersoneria { get; set; } = "JURIDICA";
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Tratamiento { get; set; }
    public string? Profesion { get; set; }
    public long? EstadoPersonaId { get; set; }
    public string? EstadoPersonaDescripcion { get; set; }
    public long? EstadoCivilId { get; set; }
    public string? EstadoCivil { get; set; }
    public string? EstadoCivilDescripcion { get; set; }
    public string? Nacionalidad { get; set; }
    public string? Sexo { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public bool EsEntidadGubernamental { get; set; }
    public string? ClaveFiscal { get; set; }
    public string? ValorClaveFiscal { get; set; }

    // ─── Documento e IVA ──────────────────────────────────────────────────────
    public long TipoDocumentoId { get; set; }
    public string TipoDocumentoDescripcion { get; set; } = string.Empty;
    public string NroDocumento { get; set; } = string.Empty;
    public long CondicionIvaId { get; set; }
    public string CondicionIvaDescripcion { get; set; } = string.Empty;

    // ─── Clasificación / Roles ────────────────────────────────────────────────
    public long? CategoriaId { get; set; }
    public string? CategoriaDescripcion { get; set; }
    public long? CategoriaClienteId { get; set; }
    public string? CategoriaClienteDescripcion { get; set; }
    public long? EstadoClienteId { get; set; }
    public string? EstadoClienteDescripcion { get; set; }
    public bool? EstadoClienteBloquea { get; set; }
    public long? CategoriaProveedorId { get; set; }
    public string? CategoriaProveedorDescripcion { get; set; }
    public long? EstadoProveedorId { get; set; }
    public string? EstadoProveedorDescripcion { get; set; }
    public bool? EstadoProveedorBloquea { get; set; }
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
    public long? PaisId { get; set; }
    public string? PaisDescripcion { get; set; }
    public long? ProvinciaId { get; set; }
    public string? ProvinciaDescripcion { get; set; }
    public long? LocalidadId { get; set; }
    public string? LocalidadDescripcion { get; set; }
    public long? BarrioId { get; set; }
    public string? BarrioDescripcion { get; set; }
    public string GeografiaCompleta { get; set; } = string.Empty;
    public string UbicacionCompleta { get; set; } = string.Empty;
    public string Completo { get; set; } = string.Empty;

    // ─── Datos fiscales ───────────────────────────────────────────────────────
    public string? NroIngresosBrutos { get; set; }
    public string? NroMunicipal { get; set; }
    public long? CuentaContableId { get; set; }
    public string? CuentaContableCodigo { get; set; }
    public string? CuentaContableDescripcion { get; set; }

    // ─── Contacto ─────────────────────────────────────────────────────────────
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }
    public string? Web { get; set; }

    // ─── Comercial ────────────────────────────────────────────────────────────
    public long? MonedaId { get; set; }
    public string? MonedaDescripcion { get; set; }
    public decimal? LimiteCredito { get; set; }
    public decimal? PorcentajeMaximoDescuento { get; set; }
    public DateOnly? VigenciaCreditoDesde { get; set; }
    public DateOnly? VigenciaCreditoHasta { get; set; }
    public TerceroCuentaCorrienteDto CuentaCorriente { get; set; } = new();
    public bool Facturable { get; set; }
    public long? CobradorId { get; set; }
    public string? CobradorUserName { get; set; }
    public string? CobradorNombre { get; set; }
    public bool AplicaComisionCobrador { get; set; }
    public decimal PctComisionCobrador { get; set; }
    public long? VendedorId { get; set; }
    public string? VendedorUserName { get; set; }
    public string? VendedorNombre { get; set; }
    public bool AplicaComisionVendedor { get; set; }
    public decimal PctComisionVendedor { get; set; }
    public string? Observacion { get; set; }
    public bool AccesoUsuarioCliente { get; set; }
    public string? UsuarioClienteUserName { get; set; }
    public string? UsuarioClienteGrupoUserName { get; set; }
    public bool TieneUsuarioCliente { get; set; }
    public bool UsuarioClienteActivo { get; set; }
    public TerceroUsuarioClienteDto? UsuarioCliente { get; set; }
    public TerceroPerfilComercialDto PerfilComercial { get; set; } = new();
    public IReadOnlyList<TerceroMedioContactoDto> MediosContacto { get; set; } = [];
    public IReadOnlyList<TerceroDomicilioDto> Domicilios { get; set; } = [];
    public IReadOnlyList<TerceroContactoDto> Contactos { get; set; } = [];
    public IReadOnlyList<TerceroSucursalEntregaDto> SucursalesEntrega { get; set; } = [];
    public TerceroSucursalEntregaDto? SucursalEntregaPrincipal { get; set; }
    public bool RequiereDefinirEntrega { get; set; }
    public IReadOnlyList<TerceroTransporteDto> Transportes { get; set; } = [];
    public IReadOnlyList<TerceroVentanaCobranzaDto> VentanasCobranza { get; set; } = [];

    // ─── Control ──────────────────────────────────────────────────────────────
    public long? SucursalId { get; set; }
    public string? SucursalDescripcion { get; set; }
    public bool Activo { get; set; }
    public string EstadoVisibleDescripcion { get; set; } = "Activo";
    public bool EstadoVisibleBloquea { get; set; }
    public string EstadoOperativo { get; set; } = "ACTIVO";
    public string EstadoOperativoDescripcion { get; set; } = "Activo";
    public bool EstadoOperativoBloquea { get; set; }

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
    public long? ProvinciaId { get; set; }
    public string? ProvinciaDescripcion { get; set; }
    public long? LocalidadId { get; set; }
    public string? LocalidadDescripcion { get; set; }
    public long? BarrioId { get; set; }
    public string? BarrioDescripcion { get; set; }
    public string GeografiaCompleta { get; set; } = string.Empty;
    public string UbicacionCompleta { get; set; } = string.Empty;
    public string Completo { get; set; } = string.Empty;
}
