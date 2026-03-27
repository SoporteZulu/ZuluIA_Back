using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public record GetComprobantesSifenPendientesQuery(
    int Page = 1,
    int PageSize = 50,
    long? SucursalId = null,
    string? EstadoSifen = null,
    string? CodigoRespuesta = null,
    bool? PuedeReintentar = null,
    bool SoloConIdentificadores = false,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null,
    string? SortBy = null)
    : IRequest<PagedResult<ComprobanteSifenPendienteDto>>;

public class GetComprobantesSifenPendientesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetComprobantesSifenPendientesQuery, PagedResult<ComprobanteSifenPendienteDto>>
{
    public async Task<PagedResult<ComprobanteSifenPendienteDto>> Handle(
        GetComprobantesSifenPendientesQuery request,
        CancellationToken ct)
    {
        if (request.Page <= 0 || request.PageSize <= 0)
            return PagedResult<ComprobanteSifenPendienteDto>.Empty(Math.Max(request.Page, 1), Math.Max(request.PageSize, 1));

        var query =
            from comprobante in db.Comprobantes.AsNoTracking()
            join sucursal in db.Sucursales.AsNoTracking() on comprobante.SucursalId equals sucursal.Id
            join pais in db.Paises.AsNoTracking() on sucursal.PaisId equals pais.Id
            where comprobante.Estado == EstadoComprobante.Emitido
                && (pais.Codigo == "PY" || pais.Codigo == "PRY")
                && (comprobante.EstadoSifen == null || comprobante.EstadoSifen != EstadoSifenParaguay.Aceptado)
            select comprobante;

        if (request.SucursalId.HasValue)
            query = query.Where(x => x.SucursalId == request.SucursalId.Value);

        if (!string.IsNullOrWhiteSpace(request.EstadoSifen)
            && Enum.TryParse<EstadoSifenParaguay>(request.EstadoSifen, true, out var estadoSifen))
        {
            query = query.Where(x => x.EstadoSifen == estadoSifen);
        }

        if (!string.IsNullOrWhiteSpace(request.CodigoRespuesta))
        {
            var codigo = request.CodigoRespuesta.Trim();
            query = query.Where(x => x.SifenCodigoRespuesta == codigo);
        }

        if (request.PuedeReintentar.HasValue)
        {
            if (request.PuedeReintentar.Value)
            {
                query = query.Where(x => x.EstadoSifen == EstadoSifenParaguay.Rechazado || x.EstadoSifen == EstadoSifenParaguay.Error);
            }
            else
            {
                query = query.Where(x => x.EstadoSifen == null || x.EstadoSifen == EstadoSifenParaguay.Pendiente);
            }
        }

        if (request.FechaDesde.HasValue)
            query = query.Where(x => x.Fecha >= request.FechaDesde.Value);

        if (request.FechaHasta.HasValue)
            query = query.Where(x => x.Fecha <= request.FechaHasta.Value);

        if (request.SoloConIdentificadores)
        {
            query = query.Where(x => x.SifenTrackingId != null || x.SifenCdc != null || x.SifenNumeroLote != null);
        }

        var totalCount = await query.CountAsync(ct);
        query = ApplySorting(query, request.SortBy);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ComprobanteSifenPendienteDto
            {
                ComprobanteId = x.Id,
                SucursalId = x.SucursalId,
                TerceroId = x.TerceroId,
                Prefijo = x.Numero.Prefijo,
                Numero = x.Numero.Numero,
                Fecha = x.Fecha,
                EstadoComprobante = x.Estado,
                EstadoSifen = x.EstadoSifen,
                SifenCodigoRespuesta = x.SifenCodigoRespuesta,
                SifenMensajeRespuesta = x.SifenMensajeRespuesta,
                SifenTrackingId = x.SifenTrackingId,
                SifenCdc = x.SifenCdc,
                SifenNumeroLote = x.SifenNumeroLote,
                SifenFechaRespuesta = x.SifenFechaRespuesta,
                TimbradoId = x.TimbradoId,
                NroTimbrado = x.NroTimbrado,
                TieneIdentificadores = x.SifenTrackingId != null
                    || x.SifenCdc != null
                    || x.SifenNumeroLote != null,
                PuedeReintentar = x.EstadoSifen == EstadoSifenParaguay.Rechazado
                    || x.EstadoSifen == EstadoSifenParaguay.Error,
                PuedeConciliar = x.EstadoSifen != EstadoSifenParaguay.Aceptado
                    && (x.SifenTrackingId != null || x.SifenCdc != null || x.SifenNumeroLote != null)
            })
            .ToListAsync(ct);

        return new PagedResult<ComprobanteSifenPendienteDto>(items, request.Page, request.PageSize, totalCount);
    }

    private static IQueryable<Domain.Entities.Comprobantes.Comprobante> ApplySorting(
        IQueryable<Domain.Entities.Comprobantes.Comprobante> query,
        string? sortBy)
    {
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "fechaasc" => query.OrderBy(x => x.Fecha).ThenBy(x => x.Id),
            "fechadesc" => query.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id),
            "estadoasc" => query.OrderBy(x => x.EstadoSifen).ThenByDescending(x => x.SifenFechaRespuesta ?? x.CreatedAt).ThenByDescending(x => x.Id),
            "estadodesc" => query.OrderByDescending(x => x.EstadoSifen).ThenByDescending(x => x.SifenFechaRespuesta ?? x.CreatedAt).ThenByDescending(x => x.Id),
            _ => query.OrderByDescending(x => x.SifenFechaRespuesta ?? x.CreatedAt).ThenByDescending(x => x.Id)
        };
    }
}