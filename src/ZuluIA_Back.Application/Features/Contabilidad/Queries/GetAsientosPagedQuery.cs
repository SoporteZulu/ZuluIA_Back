using MediatR;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Contabilidad.Queries;

public record GetAsientosPagedQuery(
    int Page,
    int PageSize,
    long EjercicioId,
    long? SucursalId,
    EstadoAsiento? Estado,
    DateOnly? Desde,
    DateOnly? Hasta)
    : IRequest<PagedResult<AsientoListDto>>;