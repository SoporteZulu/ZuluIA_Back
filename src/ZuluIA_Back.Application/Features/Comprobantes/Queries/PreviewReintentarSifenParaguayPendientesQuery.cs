using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public record PreviewReintentarSifenParaguayPendientesQuery(
    int MaxItems = 20,
    long? SucursalId = null,
    string? EstadoSifen = null,
    string? CodigoRespuesta = null,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null)
    : IRequest<ReintentoSifenParaguayPreviewDto>;

public class PreviewReintentarSifenParaguayPendientesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<PreviewReintentarSifenParaguayPendientesQuery, ReintentoSifenParaguayPreviewDto>
{
    public async Task<ReintentoSifenParaguayPreviewDto> Handle(
        PreviewReintentarSifenParaguayPendientesQuery request,
        CancellationToken ct)
    {
        if (request.MaxItems <= 0)
        {
            return new ReintentoSifenParaguayPreviewDto
            {
                MaxItems = 0,
                SucursalId = request.SucursalId,
                Encontrados = 0,
                TotalElegibles = 0,
                HayMasResultados = false
            };
        }

        EstadoSifenParaguay? estadoSifen = null;
        if (!string.IsNullOrWhiteSpace(request.EstadoSifen)
            && Enum.TryParse<EstadoSifenParaguay>(request.EstadoSifen, true, out var parsedEstadoSifen))
        {
            estadoSifen = parsedEstadoSifen;
        }

        var codigoRespuesta = string.IsNullOrWhiteSpace(request.CodigoRespuesta)
            ? null
            : request.CodigoRespuesta.Trim();

        var query =
            from comprobante in db.Comprobantes.AsNoTracking()
            join sucursal in db.Sucursales.AsNoTracking() on comprobante.SucursalId equals sucursal.Id
            join pais in db.Paises.AsNoTracking() on sucursal.PaisId equals pais.Id
            where comprobante.Estado == EstadoComprobante.Emitido
                && (pais.Codigo == "PY" || pais.Codigo == "PRY")
                && (comprobante.EstadoSifen == EstadoSifenParaguay.Rechazado || comprobante.EstadoSifen == EstadoSifenParaguay.Error)
                && (!request.SucursalId.HasValue || comprobante.SucursalId == request.SucursalId.Value)
                && (!estadoSifen.HasValue || comprobante.EstadoSifen == estadoSifen.Value)
                && (codigoRespuesta == null || comprobante.SifenCodigoRespuesta == codigoRespuesta)
                && (!request.FechaDesde.HasValue || comprobante.Fecha >= request.FechaDesde.Value)
                && (!request.FechaHasta.HasValue || comprobante.Fecha <= request.FechaHasta.Value)
            select comprobante;

        var totalElegibles = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.SifenFechaRespuesta)
            .ThenBy(x => x.Id)
            .Select(comprobante => new ReintentoSifenParaguayPreviewItemDto
            {
                ComprobanteId = comprobante.Id,
                SucursalId = comprobante.SucursalId,
                Fecha = comprobante.Fecha,
                EstadoSifen = comprobante.EstadoSifen,
                CodigoRespuesta = comprobante.SifenCodigoRespuesta,
                MensajeRespuesta = comprobante.SifenMensajeRespuesta,
                TrackingId = comprobante.SifenTrackingId,
                Cdc = comprobante.SifenCdc,
                NumeroLote = comprobante.SifenNumeroLote,
                FechaRespuesta = comprobante.SifenFechaRespuesta
            })
            .Take(request.MaxItems)
            .ToListAsync(ct);

        return new ReintentoSifenParaguayPreviewDto
        {
            MaxItems = request.MaxItems,
            SucursalId = request.SucursalId,
            Encontrados = items.Count,
            TotalElegibles = totalElegibles,
            HayMasResultados = totalElegibles > items.Count,
            Items = items,
            Estados = items
                .GroupBy(x => x.EstadoSifen)
                .Select(group => new ReintentoSifenParaguayPreviewEstadoResumenDto
                {
                    EstadoSifen = group.Key,
                    Cantidad = group.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ThenBy(x => x.EstadoSifen)
                .ToList(),
            CodigosRespuesta = items
                .Where(x => !string.IsNullOrWhiteSpace(x.CodigoRespuesta))
                .GroupBy(x => x.CodigoRespuesta!)
                .Select(group => new ReintentoSifenParaguayPreviewCodigoResumenDto
                {
                    CodigoRespuesta = group.Key,
                    Cantidad = group.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ThenBy(x => x.CodigoRespuesta)
                .ToList(),
            MensajesRespuesta = items
                .Where(x => !string.IsNullOrWhiteSpace(x.MensajeRespuesta))
                .GroupBy(x => x.MensajeRespuesta!)
                .Select(group => new ReintentoSifenParaguayPreviewMensajeResumenDto
                {
                    MensajeRespuesta = group.Key,
                    Cantidad = group.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ThenBy(x => x.MensajeRespuesta)
                .ToList()
        };
    }
}