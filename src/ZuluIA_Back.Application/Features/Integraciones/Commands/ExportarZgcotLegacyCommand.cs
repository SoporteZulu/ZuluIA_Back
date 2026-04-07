using MediatR;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public record ExportarZgcotLegacyCommand(DateOnly Desde, DateOnly Hasta, long? SucursalId, FormatoExportacionReporte Formato = FormatoExportacionReporte.Csv, string? ClaveIdempotencia = null) : IRequest<Result<ExportacionReporteDto>>;
