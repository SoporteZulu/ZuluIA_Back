using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

/// <summary>
/// Crea un nuevo tercero (cliente, proveedor o ambos).
/// Equivalente al flujo agregarNuevo() → cargarDatos() → Guardar()
/// del formulario de ABM de Clientes/Proveedores del VB6.
/// Retorna Result<long> con el Id generado en caso de éxito.
/// </summary>
public record CreateTerceroCommand(
    // ─── Identificación obligatoria ───────────────────────────────────────────
    string? Legajo,
    string RazonSocial,
    string? NombreFantasia,
    string? TipoPersoneria,
    string? Nombre,
    string? Apellido,
    string? Tratamiento,
    string? Profesion,
    long? EstadoPersonaId,
    long? EstadoCivilId,
    string? EstadoCivil,
    string? Nacionalidad,
    string? Sexo,
    DateOnly? FechaNacimiento,
    DateOnly? FechaRegistro,
    bool EsEntidadGubernamental,
    string? ClaveFiscal,
    string? ValorClaveFiscal,

    // ─── Documento e IVA ──────────────────────────────────────────────────────
    long? TipoDocumentoId,
    string? NroDocumento,
    long CondicionIvaId,

    // ─── Roles (al menos uno requerido) ──────────────────────────────────────
    bool EsCliente,
    bool EsProveedor,
    bool EsEmpleado,

    // ─── Domicilio ────────────────────────────────────────────────────────────
    string? Calle,
    string? Nro,
    string? Piso,
    string? Dpto,
    string? CodigoPostal,
    long? PaisId,
    long? ProvinciaId,
    long? LocalidadId,
    long? BarrioId,

    // ─── Datos fiscales ───────────────────────────────────────────────────────
    string? NroIngresosBrutos,
    string? NroMunicipal,

    // ─── Contacto ─────────────────────────────────────────────────────────────
    string? Telefono,
    string? Celular,
    string? Email,
    string? Web,

    // ─── Comercial ────────────────────────────────────────────────────────────
    long? MonedaId,
    long? CategoriaId,
    long? CategoriaClienteId,
    long? EstadoClienteId,
    long? CategoriaProveedorId,
    long? EstadoProveedorId,
    decimal? LimiteCredito,
    decimal? PorcentajeMaximoDescuento,
    DateOnly? VigenciaCreditoDesde,
    DateOnly? VigenciaCreditoHasta,
    bool Facturable,
    long? CobradorId,
    bool AplicaComisionCobrador,
    decimal PctComisionCobrador,
    long? VendedorId,
    bool AplicaComisionVendedor,
    decimal PctComisionVendedor,
    string? Observacion,

    // ─── Asignación ───────────────────────────────────────────────────────────
    long? SucursalId = null,

    // ─── Bloques funcionales ampliados ───────────────────────────────────────
    TerceroPerfilComercialPayload? PerfilComercial = null,
    IReadOnlyList<ReplaceTerceroDomicilioItem>? Domicilios = null,
    IReadOnlyList<ReplaceTerceroContactoItem>? Contactos = null,
    IReadOnlyList<ReplaceTerceroSucursalEntregaItem>? SucursalesEntrega = null,
    IReadOnlyList<ReplaceTerceroTransporteItem>? Transportes = null,
    IReadOnlyList<ReplaceTerceroVentanaCobranzaItem>? VentanasCobranza = null
) : IRequest<Result<long>>;
