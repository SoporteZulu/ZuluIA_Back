using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Domain.Interfaces;

/// <summary>
/// Contrato de acceso a datos para la entidad Tercero.
/// Extiende IRepository&lt;Tercero&gt; con consultas específicas del negocio,
/// derivadas de las operaciones que el VB6 realizaba directamente con SQL.
/// </summary>
public interface ITerceroRepository : IRepository<Tercero>
{
    // ─── Búsquedas por identificador de negocio ──────────────────────────────

    /// <summary>
    /// Obtiene un tercero por su legajo (case-insensitive).
    /// VB6: SELECT * FROM CLI_CLIENTES WHERE CLI_LEGAJO = @legajo
    /// </summary>
    Task<Tercero?> GetByLegajoAsync(
        string legajo,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene un tercero por su número de documento.
    /// VB6: SELECT * FROM CLI_CLIENTES WHERE CLI_NRO_DOC = @nroDoc
    /// </summary>
    Task<Tercero?> GetByNroDocumentoAsync(
        string nroDocumento,
        CancellationToken ct = default);

    // ─── Listado paginado con filtros ────────────────────────────────────────

    /// <summary>
    /// Devuelve un resultado paginado con filtros combinables.
    /// Equivalente al ABM de Clientes/Proveedores del VB6 con sus combos de filtro.
    /// </summary>
    Task<PagedResult<Tercero>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        bool? soloClientes,
        bool? soloProveedores,
        bool? soloEmpleados,
        bool? soloActivos,
        long? condicionIvaId,
        long? categoriaId,
        long? sucursalId,
        CancellationToken ct = default);

    // ─── Validaciones de unicidad ─────────────────────────────────────────────

    /// <summary>
    /// Verifica si ya existe un tercero con ese legajo,
    /// opcionalmente excluyendo un ID para la edición (UPDATE).
    /// VB6: validarDatos() verificaba duplicados de legajo antes de guardar.
    /// </summary>
    Task<bool> ExisteLegajoAsync(
        string legajo,
        long? excludeId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Verifica si ya existe un tercero con ese número de documento,
    /// opcionalmente excluyendo un ID para la edición (UPDATE).
    /// VB6: validarDatos() verificaba duplicados de CUIT/DNI antes de guardar.
    /// </summary>
    Task<bool> ExisteNroDocumentoAsync(
        string nroDocumento,
        long? excludeId = null,
        CancellationToken ct = default);

    // ─── Validaciones de integridad referencial ───────────────────────────────

    /// <summary>
    /// Verifica si el tercero tiene comprobantes asociados (ventas o compras).
    /// VB6: validarEliminar() bloqueaba la baja si existían comprobantes.
    /// Se usa en DeleteTerceroCommandHandler antes de llamar a Desactivar().
    /// </summary>
    Task<bool> TieneComprobantesAsync(
        long terceroId,
        CancellationToken ct = default);

    /// <summary>
    /// Verifica si el tercero tiene movimientos en cuenta corriente.
    /// Complementa la validación de TieneComprobantesAsync para
    /// cobros/pagos directos no ligados a comprobantes.
    /// </summary>
    Task<bool> TieneMovimientosCuentaCorrienteAsync(
        long terceroId,
        CancellationToken ct = default);

    /// <summary>
    /// Verifica si el tercero tiene un empleado asociado en la tabla empleados.
    /// Necesario para bloquear la baja cuando EsEmpleado = true y hay legajo laboral.
    /// </summary>
    Task<bool> TieneEmpleadoActivoAsync(
        long terceroId,
        CancellationToken ct = default);

    // ─── Consultas para selectors / combos ───────────────────────────────────

    /// <summary>
    /// Devuelve la lista completa de clientes activos, ordenada por RazonSocial.
    /// Equivalente al llenarCombo() del VB6 en formularios de comprobantes.
    /// Optimizado con AsNoTracking y proyección mínima (solo Id + RazonSocial + Legajo).
    /// </summary>
    Task<IReadOnlyList<Tercero>> GetClientesActivosAsync(
        long? sucursalId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Devuelve la lista completa de proveedores activos, ordenada por RazonSocial.
    /// Equivalente al llenarCombo() del VB6 en formularios de órdenes de compra.
    /// </summary>
    Task<IReadOnlyList<Tercero>> GetProveedoresActivosAsync(
        long? sucursalId = null,
        CancellationToken ct = default);
}