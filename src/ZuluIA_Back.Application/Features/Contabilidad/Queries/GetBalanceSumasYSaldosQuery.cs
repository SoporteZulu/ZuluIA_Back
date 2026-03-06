using MediatR;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;

namespace ZuluIA_Back.Application.Features.Contabilidad.Queries;

public record GetBalanceSumasYSaldosQuery(
    long EjercicioId,
    long? SucursalId,
    DateOnly Desde,
    DateOnly Hasta)
    : IRequest<BalanceSumasYSaldosDto>;