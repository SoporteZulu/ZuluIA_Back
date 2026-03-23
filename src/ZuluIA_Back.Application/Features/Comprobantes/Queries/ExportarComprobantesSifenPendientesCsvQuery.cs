using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.ExportacionesFiscales.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public record ExportarComprobantesSifenPendientesCsvQuery(
    long? SucursalId = null,
    string? EstadoSifen = null,
    string? CodigoRespuesta = null,
    bool? PuedeReintentar = null,
    bool SoloConIdentificadores = false,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null,
    string? SortBy = null)
    : IRequest<ExportacionArchivoResultDto>;

public class ExportarComprobantesSifenPendientesCsvQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ExportarComprobantesSifenPendientesCsvQuery, ExportacionArchivoResultDto>
{
    public async Task<ExportacionArchivoResultDto> Handle(
        ExportarComprobantesSifenPendientesCsvQuery request,
        CancellationToken ct)
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

        query = ApplySorting(query, request.SortBy);

        var comprobantes = await query.ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("comprobanteId,sucursalId,terceroId,prefijo,numero,fecha,estadoComprobante,estadoSifen,codigoRespuesta,mensajeRespuesta,trackingId,cdc,numeroLote,fechaRespuesta,timbradoId,nroTimbrado,tieneIdentificadores,puedeReintentar,puedeConciliar");

        foreach (var comprobante in comprobantes)
        {
            var tieneIdentificadores = comprobante.SifenTrackingId != null
                || comprobante.SifenCdc != null
                || comprobante.SifenNumeroLote != null;
            var puedeReintentar = comprobante.EstadoSifen == EstadoSifenParaguay.Rechazado
                || comprobante.EstadoSifen == EstadoSifenParaguay.Error;
            var puedeConciliar = comprobante.EstadoSifen != EstadoSifenParaguay.Aceptado
                && tieneIdentificadores;

            sb.AppendLine(string.Join(",",
                comprobante.Id,
                comprobante.SucursalId,
                comprobante.TerceroId,
                comprobante.Numero.Prefijo,
                comprobante.Numero.Numero,
                EscapeCsv(comprobante.Fecha.ToString("yyyy-MM-dd")),
                EscapeCsv(comprobante.Estado.ToString()),
                EscapeCsv(comprobante.EstadoSifen?.ToString()),
                EscapeCsv(comprobante.SifenCodigoRespuesta),
                EscapeCsv(comprobante.SifenMensajeRespuesta),
                EscapeCsv(comprobante.SifenTrackingId),
                EscapeCsv(comprobante.SifenCdc),
                EscapeCsv(comprobante.SifenNumeroLote),
                EscapeCsv(comprobante.SifenFechaRespuesta?.ToString("O")),
                EscapeCsv(comprobante.TimbradoId?.ToString()),
                EscapeCsv(comprobante.NroTimbrado),
                tieneIdentificadores ? "true" : "false",
                puedeReintentar ? "true" : "false",
                puedeConciliar ? "true" : "false"));
        }

        var nombreArchivo = $"SIFEN_PENDIENTES_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return new ExportacionArchivoResultDto(nombreArchivo, sb.ToString(), comprobantes.Count);
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

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var escaped = value.Replace("\"", "\"\"");
        return escaped.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{escaped}\""
            : escaped;
    }
}