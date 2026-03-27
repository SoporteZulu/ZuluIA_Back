using MediatR;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class ExportarZgcotLegacyCommandHandler(LegacyExportacionService service) : IRequestHandler<ExportarZgcotLegacyCommand, Result<ExportacionReporteDto>>
{
    public Task<Result<ExportacionReporteDto>> Handle(ExportarZgcotLegacyCommand request, CancellationToken ct)
        => service.ExportarZgcotAsync(request, ct);
}
