using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.ExportacionesFiscales.DTOs;

namespace ZuluIA_Back.Application.Features.ExportacionesFiscales.Queries;

/// <summary>Exporta compras en formato CITI Compras AFIP.</summary>
public record ExportarCitiComprasQuery(long SucursalId, int Anio, int Mes)
    : IRequest<ExportacionArchivoResultDto>;

public class ExportarCitiComprasQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ExportarCitiComprasQuery, ExportacionArchivoResultDto>
{
    public async Task<ExportacionArchivoResultDto> Handle(
        ExportarCitiComprasQuery request, CancellationToken ct)
    {
        var desde = new DateOnly(request.Anio, request.Mes, 1);
        var hasta = desde.AddMonths(1).AddDays(-1);

        // Pagos como representación de compras (comprobantes de tipo compra)
        var pagos = await db.Pagos.AsNoTracking()
            .Where(p => p.SucursalId == request.SucursalId
                     && p.Fecha >= desde && p.Fecha <= hasta
                     && p.DeletedAt == null)
            .OrderBy(p => p.Fecha).ThenBy(p => p.Id)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        foreach (var p in pagos)
        {
            sb.AppendLine(string.Join("|",
                p.Fecha.ToString("yyyyMMdd"),
                "001",
                "0001",
                p.Id.ToString("D8"),
                string.Empty,
                "80",
                "00000000000",
                string.Empty,
                p.Total.ToString("F2"),
                "0.00",
                "0.00",
                "0.00",
                "0.00",
                "0.00",
                "0.00",
                "1",
                string.Empty,
                "0.00",
                "0.00",
                "0.00",
                "0.00",
                string.Empty,
                string.Empty,
                "0.00"));
        }

        var nombreArchivo = $"CITI_COMPRAS_{request.Anio}{request.Mes:D2}_S{request.SucursalId}.txt";
        return new ExportacionArchivoResultDto(nombreArchivo, sb.ToString(), pagos.Count);
    }
}
