using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public record GetComprobantesSifenPendientesResumenQuery(
    long? SucursalId = null,
    string? EstadoSifen = null,
    string? CodigoRespuesta = null,
    bool? PuedeReintentar = null,
    bool SoloConIdentificadores = false,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null,
    int TopCodigos = 10,
    int TopMensajes = 10)
    : IRequest<ComprobanteSifenPendientesResumenDto>;

public class GetComprobantesSifenPendientesResumenQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetComprobantesSifenPendientesResumenQuery, ComprobanteSifenPendientesResumenDto>
{
    public async Task<ComprobanteSifenPendientesResumenDto> Handle(
        GetComprobantesSifenPendientesResumenQuery request,
        CancellationToken ct)
    {
        var query = BuildFilteredQuery(request);

        var total = await query.CountAsync(ct);
        var reintentables = await query.CountAsync(
            x => x.EstadoSifen == EstadoSifenParaguay.Rechazado || x.EstadoSifen == EstadoSifenParaguay.Error,
            ct);
        var conIdentificadores = await query.CountAsync(
            x => x.SifenTrackingId != null || x.SifenCdc != null || x.SifenNumeroLote != null,
            ct);
        var conciliables = await query.CountAsync(
            x => (x.SifenTrackingId != null || x.SifenCdc != null || x.SifenNumeroLote != null)
                && x.EstadoSifen != EstadoSifenParaguay.Aceptado,
            ct);
        var sinEstado = await query.CountAsync(x => x.EstadoSifen == null, ct);
        var topCodigos = Math.Max(request.TopCodigos, 0);
        var topMensajes = Math.Max(request.TopMensajes, 0);
        var estados = await query
            .GroupBy(x => x.EstadoSifen)
            .Select(group => new ComprobanteSifenPendientesEstadoResumenDto
            {
                EstadoSifen = group.Key,
                Cantidad = group.Count()
            })
            .OrderBy(x => x.EstadoSifen)
            .ToListAsync(ct);
        var codigosRespuesta = await query
            .Where(x => x.SifenCodigoRespuesta != null && x.SifenCodigoRespuesta != string.Empty)
            .GroupBy(x => x.SifenCodigoRespuesta!)
            .Select(group => new ComprobanteSifenPendientesCodigoResumenDto
            {
                CodigoRespuesta = group.Key,
                Cantidad = group.Count()
            })
            .OrderByDescending(x => x.Cantidad)
            .ThenBy(x => x.CodigoRespuesta)
            .Take(topCodigos)
            .ToListAsync(ct);
        var mensajesRespuesta = await query
            .Where(x => x.SifenMensajeRespuesta != null && x.SifenMensajeRespuesta != string.Empty)
            .GroupBy(x => x.SifenMensajeRespuesta!)
            .Select(group => new ComprobanteSifenPendientesMensajeResumenDto
            {
                MensajeRespuesta = group.Key,
                Cantidad = group.Count()
            })
            .OrderByDescending(x => x.Cantidad)
            .ThenBy(x => x.MensajeRespuesta)
            .Take(topMensajes)
            .ToListAsync(ct);

        return new ComprobanteSifenPendientesResumenDto
        {
            Total = total,
            Reintentables = reintentables,
            ConIdentificadores = conIdentificadores,
            Conciliables = conciliables,
            SinEstadoSifen = sinEstado,
            Estados = estados,
            CodigosRespuesta = codigosRespuesta,
            MensajesRespuesta = mensajesRespuesta
        };
    }

    private IQueryable<Domain.Entities.Comprobantes.Comprobante> BuildFilteredQuery(GetComprobantesSifenPendientesResumenQuery request)
    {
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
            query = query.Where(x => x.SifenTrackingId != null || x.SifenCdc != null || x.SifenNumeroLote != null);

        return query;
    }
}