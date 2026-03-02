using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Consulta paginada de terceros con filtros combinables.
/// Equivalente a la grilla principal del ABM de Clientes/Proveedores del VB6
/// con todos sus combos de filtro: tipo, condición IVA, sucursal, estado.
/// </summary>
public record GetTercerosPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,

    // ─── Filtros de rol ───────────────────────────────────────────────────────
    bool? SoloClientes = null,
    bool? SoloProveedores = null,
    bool? SoloEmpleados = null,

    // ─── Filtros dimensionales ────────────────────────────────────────────────
    bool? SoloActivos = true,   // Por defecto muestra solo activos
    long? CondicionIvaId = null,
    long? CategoriaId = null,
    long? SucursalId = null
) : IRequest<PagedResult<TerceroListDto>>;