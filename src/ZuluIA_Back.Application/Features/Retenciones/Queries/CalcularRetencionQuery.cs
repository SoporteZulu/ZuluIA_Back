using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Retenciones.Queries;

/// <summary>
/// Calcula el importe de retención para una base imponible dada,
/// usando las escalas configuradas del tipo de retención.
/// </summary>
public record CalcularRetencionQuery(long TipoRetencionId, decimal BaseImponible)
    : IRequest<Result<decimal>>;
