using MediatR;
using ZuluIA_Back.Application.Features.Finanzas.DTOs;

namespace ZuluIA_Back.Application.Features.Finanzas.Queries;

public record GetCuentaCorrienteTerceroQuery(
    long TerceroId,
    long? SucursalId)
    : IRequest<IReadOnlyList<CuentaCorrienteDto>>;