using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class TerceroRepository(AppDbContext context)
    : BaseRepository<Tercero>(context), ITerceroRepository
{
    // ─── Búsquedas por identificador de negocio ────────────��─────────────────

    public async Task<Tercero?> GetByLegajoAsync(
        string legajo,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Legajo == legajo.Trim().ToUpperInvariant(), ct);

    public async Task<Tercero?> GetByNroDocumentoAsync(
        string nroDocumento,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.NroDocumento == nroDocumento.Trim(), ct);

    // ─── Listado paginado con filtros ────────────────────────────────────────

    public async Task<PagedResult<Tercero>> GetPagedAsync(
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
        CancellationToken ct = default)
    {
        // Base: excluye los eliminados de forma permanente (deleted_at no nulo)
        // soloActivos null → muestra activos e inactivos (útil para administración)
        // soloActivos true → solo activos
        // soloActivos false → solo inactivos/dados de baja
        var query = DbSet.AsNoTracking();

        // Filtro de baja lógica
        if (soloActivos is null || soloActivos == true)
            query = query.Where(x => !x.IsDeleted);

        if (soloActivos == false)
            query = query.Where(x => x.IsDeleted);

        // Filtro por estado activo/inactivo dentro de los no eliminados
        if (soloActivos == true)
            query = query.Where(x => x.Activo);

        if (soloActivos == false)
            query = query.Where(x => !x.Activo);

        // Filtros de rol (combinables: un tercero puede ser cliente Y proveedor)
        if (soloClientes == true)
            query = query.Where(x => x.EsCliente);

        if (soloProveedores == true)
            query = query.Where(x => x.EsProveedor);

        if (soloEmpleados == true)
            query = query.Where(x => x.EsEmpleado);

        // Filtros dimensionales
        if (condicionIvaId.HasValue)
            query = query.Where(x => x.CondicionIvaId == condicionIvaId.Value);

        if (categoriaId.HasValue)
            query = query.Where(x => x.CategoriaId == categoriaId.Value);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        // Búsqueda textual libre: razón social, legajo, nro documento,
        // nombre fantasía, email y teléfono
        // (equivalente al filtro del ABM del VB6)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.RazonSocial.ToLower().Contains(term)                              ||
                x.Legajo.ToLower().Contains(term)                                   ||
                x.NroDocumento.ToLower().Contains(term)                             ||
                (x.NombreFantasia != null && x.NombreFantasia.ToLower().Contains(term)) ||
                (x.Email         != null && x.Email.ToLower().Contains(term))       ||
                (x.Telefono      != null && x.Telefono.Contains(term))              ||
                (x.Celular       != null && x.Celular.Contains(term)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.RazonSocial)
            .ThenBy(x => x.Legajo)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Tercero>(items, page, pageSize, total);
    }

    // ─── Validaciones de unicidad ─────────────────────────────────────────────

    public async Task<bool> ExisteLegajoAsync(
        string legajo,
        long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(x => x.Legajo == legajo.Trim().ToUpperInvariant());

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<bool> ExisteNroDocumentoAsync(
        string nroDocumento,
        long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(x => x.NroDocumento == nroDocumento.Trim());

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }

    // ─── Validaciones de integridad referencial ───────────────────────────────

    /// <summary>
    /// Verifica si el tercero tiene comprobantes (ventas o compras) asociados.
    /// Equivalente al validarEliminar() del VB6 que hacía
    /// SELECT COUNT(*) FROM COMPROBANTES WHERE TERCERO_ID = @id
    /// </summary>
    public async Task<bool> TieneComprobantesAsync(
        long terceroId,
        CancellationToken ct = default) =>
        await Context.Comprobantes
            .AsNoTracking()
            .AnyAsync(x => x.TerceroId == terceroId, ct);

    /// DESCOMENTAR CUANDO SE CREEN LOS MODULOS CUENTA CORRIENTE Y EMPLEADOS
  
    /// <summary>
    /// Verifica si el tercero tiene movimientos en cuenta corriente.
    /// VB6 también chequeaba MOV_CTA_CTE antes de permitir la baja.
    /// </summary>
    //public async Task<bool> TieneMovimientosCuentaCorrienteAsync(
    //    long terceroId,
    //    CancellationToken ct = default) =>
    //    await Context.Set<Domain.Entities.CuentaCorriente.MovimientoCtaCte>()
    //        .AsNoTracking()
    //        .AnyAsync(x => x.TerceroId == terceroId, ct);

    /// <summary>
    /// Verifica si el tercero tiene un empleado activo asociado.
    /// Bloquea la baja del tercero si tiene legajo laboral activo.
    /// </summary>
    //public async Task<bool> TieneEmpleadoActivoAsync(
    //    long terceroId,
    //    CancellationToken ct = default) =>
    //    await Context.Set<Domain.Entities.Empleados.Empleado>()
    //        .AsNoTracking()
    //        .AnyAsync(x => x.TerceroId == terceroId && x.Activo, ct);

    /// DESCOMENTAR CUANDO SE CREEN LOS MODULOS CUENTA CORRIENTE Y EMPLEADOS
    /// 
    // Alternativa temporal con SQL raw si MovimientoCtaCte/Empleado
    // aún no tienen entidad de dominio mapeada:

    public async Task<bool> TieneMovimientosCuentaCorrienteAsync(
        long terceroId,
        CancellationToken ct = default)
    {
        var result = await Context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*)::int FROM movimientos_cta_cte WHERE tercero_id = {0}",
                terceroId)
            .FirstAsync(ct);
        return result > 0;
    }

    public async Task<bool> TieneEmpleadoActivoAsync(
        long terceroId,
        CancellationToken ct = default)
    {
        var result = await Context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*)::int FROM empleados WHERE tercero_id = {0} AND activo = true",
                terceroId)
            .FirstAsync(ct);
        return result > 0;
    }
    // ─── Consultas para selectors / combos ───────────────────────────────────

    /// <summary>
    /// Lista de clientes activos para combos y selectores.
    /// Equivalente al llenarCombo() de clientes del VB6.
    /// </summary>
    public async Task<IReadOnlyList<Tercero>> GetClientesActivosAsync(
        long? sucursalId = null,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(x => x.EsCliente && x.Activo && !x.IsDeleted);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value
                                  || x.SucursalId == null);

        return await query
            .OrderBy(x => x.RazonSocial)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Lista de proveedores activos para combos y selectores.
    /// Equivalente al llenarCombo() de proveedores del VB6.
    /// </summary>
    public async Task<IReadOnlyList<Tercero>> GetProveedoresActivosAsync(
        long? sucursalId = null,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(x => x.EsProveedor && x.Activo && !x.IsDeleted);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value
                                  || x.SucursalId == null);

        return await query
            .OrderBy(x => x.RazonSocial)
            .ToListAsync(ct);
    }
}