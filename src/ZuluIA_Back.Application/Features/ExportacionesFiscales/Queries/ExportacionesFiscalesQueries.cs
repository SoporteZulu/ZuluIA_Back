using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.ExportacionesFiscales.DTOs;

namespace ZuluIA_Back.Application.Features.ExportacionesFiscales.Queries;

/// <summary>Exporta percepciones IIBB usando el campo Percepciones de Comprobante.</summary>
public record ExportarIibbPercepcionesQuery(long SucursalId, int Anio, int Mes)
    : IRequest<ExportacionArchivoResultDto>;

public class ExportarIibbPercepcionesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ExportarIibbPercepcionesQuery, ExportacionArchivoResultDto>
{
    public async Task<ExportacionArchivoResultDto> Handle(
        ExportarIibbPercepcionesQuery request, CancellationToken ct)
    {
        var desde = new DateOnly(request.Anio, request.Mes, 1);
        var hasta = desde.AddMonths(1).AddDays(-1);

        var comprobantes = await db.Comprobantes.AsNoTracking()
            .Where(c => c.SucursalId == request.SucursalId
                     && c.Fecha >= desde && c.Fecha <= hasta
                     && c.Percepciones > 0
                     && c.DeletedAt == null)
            .OrderBy(c => c.Fecha).ThenBy(c => c.Id)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        foreach (var c in comprobantes)
        {
            sb.AppendLine(string.Join("|",
                c.Fecha.ToString("yyyyMMdd"),
                c.TipoComprobanteId.ToString("D3"),
                c.Numero.Numero.ToString("D8"),
                c.Percepciones.ToString("F2")));
        }

        var nombreArchivo = $"IIBB_PERCEPCIONES_{request.Anio}{request.Mes:D2}_S{request.SucursalId}.txt";
        return new ExportacionArchivoResultDto(nombreArchivo, sb.ToString(), comprobantes.Count);
    }
}

/// <summary>Exporta retenciones de Ganancias (Sicore).</summary>
public record ExportarRetencionesGananciasQuery(long SucursalId, int Anio, int Mes)
    : IRequest<ExportacionArchivoResultDto>;

public class ExportarRetencionesGananciasQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ExportarRetencionesGananciasQuery, ExportacionArchivoResultDto>
{
    public async Task<ExportacionArchivoResultDto> Handle(
        ExportarRetencionesGananciasQuery request, CancellationToken ct)
    {
        var desde = new DateOnly(request.Anio, request.Mes, 1);
        var hasta = desde.AddMonths(1).AddDays(-1);

        var retenciones = await db.Retenciones.AsNoTracking()
            .Where(r => r.Fecha >= desde && r.Fecha <= hasta)
            .OrderBy(r => r.Fecha)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        foreach (var ret in retenciones)
        {
            sb.AppendLine(string.Join("|",
                "1",
                ret.Fecha.ToString("yyyyMMdd"),
                ret.Id.ToString(),
                "0.00",
                "0.00",
                ret.Importe.ToString("F2")));
        }

        var nombreArchivo = $"RETENCIONES_GANANCIAS_{request.Anio}{request.Mes:D2}_S{request.SucursalId}.txt";
        return new ExportacionArchivoResultDto(nombreArchivo, sb.ToString(), retenciones.Count);
    }
}

/// <summary>Exporta retenciones de IVA.</summary>
public record ExportarRetencionesIvaQuery(long SucursalId, int Anio, int Mes)
    : IRequest<ExportacionArchivoResultDto>;

public class ExportarRetencionesIvaQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ExportarRetencionesIvaQuery, ExportacionArchivoResultDto>
{
    public async Task<ExportacionArchivoResultDto> Handle(
        ExportarRetencionesIvaQuery request, CancellationToken ct)
    {
        var desde = new DateOnly(request.Anio, request.Mes, 1);
        var hasta = desde.AddMonths(1).AddDays(-1);

        var retenciones = await db.Retenciones.AsNoTracking()
            .Where(r => r.Fecha >= desde && r.Fecha <= hasta)
            .OrderBy(r => r.Fecha)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        foreach (var ret in retenciones)
        {
            sb.AppendLine(string.Join("|",
                "2",
                ret.Fecha.ToString("yyyyMMdd"),
                ret.Id.ToString(),
                ret.Importe.ToString("F2")));
        }

        var nombreArchivo = $"RETENCIONES_IVA_{request.Anio}{request.Mes:D2}_S{request.SucursalId}.txt";
        return new ExportacionArchivoResultDto(nombreArchivo, sb.ToString(), retenciones.Count);
    }
}
