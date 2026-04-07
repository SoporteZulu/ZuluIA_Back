using MediatR;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Services;

namespace ZuluIA_Back.Application.Features.Reportes.Commands;

public class ExportarReporteCommandHandler(
    ReportesService reportesService,
    ReporteExportacionService exportacionService)
    : IRequestHandler<ExportarReporteCommand, ExportacionReporteDto>
{
    public async Task<ExportacionReporteDto> Handle(ExportarReporteCommand request, CancellationToken ct)
    {
        var reporte = await reportesService.GetReporteParametrizadoAsync(
            request.TipoReporte,
            request.SucursalId,
            request.EjercicioId,
            request.Desde,
            request.Hasta,
            request.DepositoId,
            ct);

        return exportacionService.Exportar(reporte, request.Formato, request.TipoReporte.ToString().ToLowerInvariant());
    }
}
