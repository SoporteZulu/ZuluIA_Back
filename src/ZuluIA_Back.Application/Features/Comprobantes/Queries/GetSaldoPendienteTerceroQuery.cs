using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public record GetSaldoPendienteTerceroQuery(
    long TerceroId,
    long? SucursalId)
    : IRequest<IReadOnlyList<SaldoPendienteDto>>;