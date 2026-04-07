using MediatR;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class EjecutarSyncLegacyEspecificoCommandHandler(LegacyExportacionService service) : IRequestHandler<EjecutarSyncLegacyEspecificoCommand, Result<long>>
{
    public Task<Result<long>> Handle(EjecutarSyncLegacyEspecificoCommand request, CancellationToken ct)
        => service.EjecutarSyncLegacyEspecificoAsync(request, ct);
}
