using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetTercerosPagedQueryHandler(
    ITerceroRepository repo,
    IApplicationDbContext db,
    IMapper mapper)
    : IRequestHandler<GetTercerosPagedQuery, PagedResult<TerceroListDto>>
{
    public async Task<PagedResult<TerceroListDto>> Handle(
        GetTercerosPagedQuery request,
        CancellationToken ct)
    {
        // ── 1. Validar y normalizar paginación ────────────────────────────────
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        // ── 2. Obtener página de entidades desde el repositorio ───────────────
        var paged = await repo.GetPagedAsync(
            page,
            pageSize,
            request.Search,
            request.SoloClientes,
            request.SoloProveedores,
            request.SoloEmpleados,
            request.SoloActivos,
            request.CondicionIvaId,
            request.CategoriaId,
            request.SucursalId,
            ct);

        if (paged.TotalCount == 0)
            return PagedResult<TerceroListDto>.Empty(page, pageSize);

        // ── 3. Mapeo base (campos escalares + RolDisplay via AfterMap) ─────────
        var items = mapper.Map<IReadOnlyList<TerceroListDto>>(paged.Items);

        // ── 4. Resolver descripciones de FK en batch ──────────────────────────
        // CondiciónIVA
        var condicionIvaIds = paged.Items
            .Select(x => x.CondicionIvaId)
            .Distinct()
            .ToList();

        Dictionary<long, string> condicionesIva = condicionIvaIds.Count > 0
            ? await db.CondicionesIva
                .Where(x => condicionIvaIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        // Localidades (del domicilio, para columna de ubicación)
        var localidadIds = paged.Items
            .Where(x => x.Domicilio.LocalidadId.HasValue)
            .Select(x => x.Domicilio.LocalidadId!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, string> localidades = localidadIds.Count > 0
            ? await db.Localidades
                .Where(x => localidadIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        // ── 5. Enriquecer los DTOs con las descripciones ──────────────────────
        // Iteramos sobre los ítems mapeados y los originales en paralelo
        // para poder acceder a las FK del dominio sin romper el DTO.
        var itemsList = items.ToList();

        for (var i = 0; i < itemsList.Count; i++)
        {
            var dto = itemsList[i];
            var tercero = paged.Items[i];

            dto.CondicionIvaDescripcion =
                condicionesIva.TryGetValue(tercero.CondicionIvaId, out var civa)
                ? civa : string.Empty;

            if (tercero.Domicilio.LocalidadId.HasValue &&
                localidades.TryGetValue(tercero.Domicilio.LocalidadId.Value, out var loc))
                dto.LocalidadDescripcion = loc;
        }

        return new PagedResult<TerceroListDto>(itemsList, page, pageSize, paged.TotalCount);
    }
}