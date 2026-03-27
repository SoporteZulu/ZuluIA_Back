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
    decimal? LimiteCredito,
    bool Facturable,
    long? CobradorId,
    decimal PctComisionCobrador,
    long? VendedorId,
    decimal PctComisionVendedor,
    string? Observacion,

    // ─── Asignación ───────────────────────────────────────────────────────────
    long? SucursalId
) : IRequest<Result<long>>;
