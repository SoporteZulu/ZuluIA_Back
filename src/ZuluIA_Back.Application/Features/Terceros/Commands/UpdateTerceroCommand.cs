using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

/// <summary>
/// Actualiza los datos de un tercero existente.
/// Equivalente al flujo editar() → cargarDatos() → Guardar() en modo edición
/// del formulario de ABM de Clientes/Proveedores del VB6.
/// 
/// Diseño de campos:
/// - NroDocumento es opcional aquí: si viene null, NO se modifica.
///   Cambiar el documento requiere lógica adicional (validar que no haya
///   comprobantes con ese dato). Se incluye para el caso admin que lo
///   necesite explícitamente.
/// - Legajo NO se puede cambiar (es el identificador de negocio histórico).
/// </summary>
public record UpdateTerceroCommand(
    long Id,

    // ─── Identificación ───────────────────────────────────────────────────────
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
    // NroDocumento opcional: null = no modificar
    string? NroDocumento,
    long CondicionIvaId,

    // ─── Roles ────────────────────────────────────────────────────────────────
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

    // ─── Bloques funcionales ampliados ───────────────────────────────────────
    TerceroPerfilComercialPayload? PerfilComercial = null,
    IReadOnlyList<ReplaceTerceroDomicilioItem>? Domicilios = null,
    IReadOnlyList<ReplaceTerceroContactoItem>? Contactos = null,
    IReadOnlyList<ReplaceTerceroSucursalEntregaItem>? SucursalesEntrega = null,
    IReadOnlyList<ReplaceTerceroTransporteItem>? Transportes = null,
    IReadOnlyList<ReplaceTerceroVentanaCobranzaItem>? VentanasCobranza = null
) : IRequest<Result>;
