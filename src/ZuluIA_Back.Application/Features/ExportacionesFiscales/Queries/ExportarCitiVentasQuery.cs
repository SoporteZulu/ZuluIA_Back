using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.ExportacionesFiscales.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.ExportacionesFiscales.Queries;

/// <summary>Exporta ventas en formato CITI Ventas AFIP (RG 3685).</summary>
public record ExportarCitiVentasQuery(
    long SucursalId, int Anio, int Mes)
    : IRequest<ExportacionArchivoResultDto>;

public class ExportarCitiVentasQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ExportarCitiVentasQuery, ExportacionArchivoResultDto>
{
    public async Task<ExportacionArchivoResultDto> Handle(
        ExportarCitiVentasQuery request, CancellationToken ct)
    {
        var desde = new DateOnly(request.Anio, request.Mes, 1);
        var hasta = desde.AddMonths(1).AddDays(-1);

        var comprobantes = await db.Comprobantes.AsNoTracking()
            .Where(c => c.SucursalId == request.SucursalId
                     && c.Fecha >= desde && c.Fecha <= hasta
                     && c.DeletedAt == null)
            .OrderBy(c => c.Fecha).ThenBy(c => c.Id)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        foreach (var c in comprobantes)
        {
            // Formato: fecha|tipo|PV|nro|nroHasta|codDoc|nroDoc|denominacion|total|...
            sb.AppendLine(string.Join("|",
                c.Fecha.ToString("yyyyMMdd"),
                c.TipoComprobanteId.ToString("D3"),
                "0001",
                c.Numero.Numero.ToString("D8"),
                c.Numero.Numero.ToString("D8"),
                "80",
                "00000000000",
                string.Empty,
                c.Total.ToString("F2"),
                "0.00",
                "0.00",
                "0.00",
                "0.00",
                "0.00",
                "0.00",
                "0.00",
                "PES",
                "1.000000",
                "1",
                string.Empty,
                "0.00",
                c.Fecha.ToString("yyyyMMdd")));
        }

        var nombreArchivo = $"CITI_VENTAS_{request.Anio}{request.Mes:D2}_S{request.SucursalId}.txt";
        return new ExportacionArchivoResultDto(nombreArchivo, sb.ToString(), comprobantes.Count);
    }
}
