using MediatR;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class ExportarCotLegacyCommandHandler(LegacyExportacionService service) : IRequestHandler<ExportarCotLegacyCommand, Result<ExportacionReporteDto>>
{
    public Task<Result<ExportacionReporteDto>> Handle(ExportarCotLegacyCommand request, CancellationToken ct)
        => service.ExportarCotAsync(request, ct);
}
