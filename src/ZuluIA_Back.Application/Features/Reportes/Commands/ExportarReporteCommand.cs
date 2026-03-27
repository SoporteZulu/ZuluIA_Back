using MediatR;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Enums;

namespace ZuluIA_Back.Application.Features.Reportes.Commands;

public record ExportarReporteCommand(
    TipoReporteParametrizado TipoReporte,
    FormatoExportacionReporte Formato,
    long? SucursalId,
    long? EjercicioId,
    DateOnly Desde,
    DateOnly Hasta,
    long? DepositoId = null) : IRequest<ExportacionReporteDto>;
