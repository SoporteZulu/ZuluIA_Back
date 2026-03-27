using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public record EjecutarSyncLegacyEspecificoCommand(string Codigo, int RegistrosEstimados, string? PayloadResumen, string? Observacion, string? ClaveIdempotencia = null) : IRequest<Result<long>>;
