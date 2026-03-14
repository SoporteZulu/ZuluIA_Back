using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.RetencionesPorPersona.Commands;

public record AsignarRetencionAPersonaCommand(
    long TerceroId,
    long TipoRetencionId,
    string? Descripcion
) : IRequest<Result<long>>;

public record QuitarRetencionDePersonaCommand(long Id) : IRequest<Result>;
