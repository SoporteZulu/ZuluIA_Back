using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Queries;

public class GetListasPreciosPagedQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetListasPreciosPagedQuery, PagedResult<ListaPreciosDto>>
{
    public async Task<PagedResult<ListaPreciosDto>> Handle(
        GetListasPreciosPagedQuery request,
        CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var fechaReferencia = request.Fecha ?? DateOnly.FromDateTime(DateTime.Today);

        var query = db.ListasPrecios
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpperInvariant();
            query = query.Where(x =>
                x.Descripcion.ToUpper().Contains(search) ||
                (x.Observaciones != null && x.Observaciones.ToUpper().Contains(search)));
        }

        if (request.MonedaId.HasValue)
        {
            query = query.Where(x => x.MonedaId == request.MonedaId.Value);
        }

        if (request.SoloActivas.HasValue)
        {
            query = query.Where(x => x.Activa == request.SoloActivas.Value);
        }

        if (request.EsPorDefecto.HasValue)
        {
            query = query.Where(x => x.EsPorDefecto == request.EsPorDefecto.Value);
        }

        if (request.ListaPadreId.HasValue)
        {
            query = query.Where(x => x.ListaPadreId == request.ListaPadreId.Value);
        }

        if (request.SoloVigentes == true)
        {
            query = query.Where(x =>
                x.Activa &&
                (!x.VigenciaDesde.HasValue || x.VigenciaDesde.Value <= fechaReferencia) &&
                (!x.VigenciaHasta.HasValue || x.VigenciaHasta.Value >= fechaReferencia));
        }

        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0)
        {
            return PagedResult<ListaPreciosDto>.Empty(page, pageSize);
        }

        var listas = await query
            .OrderByDescending(x => x.EsPorDefecto)
            .ThenByDescending(x => x.Prioridad)
            .ThenBy(x => x.Descripcion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.Descripcion,
                x.MonedaId,
                x.VigenciaDesde,
                x.VigenciaHasta,
                x.Activa,
                x.EsPorDefecto,
                x.ListaPadreId,
                x.Prioridad,
                x.Observaciones,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync(ct);

        var listaIds = listas.Select(x => x.Id).ToList();
        var monedaIds = listas.Select(x => x.MonedaId).Distinct().ToList();
        var listaPadreIds = listas.Where(x => x.ListaPadreId.HasValue)
            .Select(x => x.ListaPadreId!.Value)
            .Distinct()
            .ToList();

        var monedas = await db.Monedas
            .AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var listasPadre = await db.ListasPrecios
            .AsNoTracking()
            .Where(x => listaPadreIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var itemsPorLista = await db.ListaPreciosItems
            .AsNoTracking()
            .Where(x => listaIds.Contains(x.ListaId))
            .GroupBy(x => x.ListaId)
            .Select(g => new { ListaId = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.ListaId, x => x.Cantidad, ct);

        var personasPorLista = await db.ListasPreciosPersonas
            .AsNoTracking()
            .Where(x => listaIds.Contains(x.ListaPreciosId))
            .GroupBy(x => x.ListaPreciosId)
            .Select(g => new { ListaId = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.ListaId, x => x.Cantidad, ct);

        var promocionesPorLista = await db.ListasPreciosPromociones
            .AsNoTracking()
            .Where(x =>
                listaIds.Contains(x.ListaId) &&
                x.Activa &&
                x.VigenciaDesde <= fechaReferencia &&
                x.VigenciaHasta >= fechaReferencia)
            .GroupBy(x => x.ListaId)
            .Select(g => new { ListaId = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.ListaId, x => x.Cantidad, ct);

        var result = listas.Select(x => new ListaPreciosDto
        {
            Id = x.Id,
            Descripcion = x.Descripcion,
            MonedaId = x.MonedaId,
            MonedaDescripcion = monedas.GetValueOrDefault(x.MonedaId)?.Descripcion,
            MonedaSimbolo = monedas.GetValueOrDefault(x.MonedaId)?.Simbolo,
            VigenciaDesde = x.VigenciaDesde,
            VigenciaHasta = x.VigenciaHasta,
            Activa = x.Activa,
            EsPorDefecto = x.EsPorDefecto,
            ListaPadreId = x.ListaPadreId,
            ListaPadreDescripcion = x.ListaPadreId.HasValue
                ? listasPadre.GetValueOrDefault(x.ListaPadreId.Value)?.Descripcion
                : null,
            Prioridad = x.Prioridad,
            Observaciones = x.Observaciones,
            EstaVigenteHoy = x.Activa
                && (!x.VigenciaDesde.HasValue || x.VigenciaDesde.Value <= fechaReferencia)
                && (!x.VigenciaHasta.HasValue || x.VigenciaHasta.Value >= fechaReferencia),
            TieneHerencia = x.ListaPadreId.HasValue,
            CantidadItems = itemsPorLista.GetValueOrDefault(x.Id),
            CantidadPersonasAsignadas = personasPorLista.GetValueOrDefault(x.Id),
            CantidadPromocionesActivas = promocionesPorLista.GetValueOrDefault(x.Id),
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        }).ToList();

        return new PagedResult<ListaPreciosDto>(result, page, pageSize, totalCount);
    }
}
