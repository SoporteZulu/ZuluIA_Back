using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public record EjecutarSyncIntegracionCommand(
    TipoProcesoIntegracion Tipo,
    string CodigoMonitor,
    string DescripcionMonitor,
    string? Observacion = null,
    string? ClaveIdempotencia = null) : IRequest<Result<long>>;
