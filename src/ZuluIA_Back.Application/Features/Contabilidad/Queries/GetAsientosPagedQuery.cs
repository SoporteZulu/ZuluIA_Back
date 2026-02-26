using MediatR;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Contabilidad.Queries;

public record GetAsientosPagedQuery(
    int Page = 1,
    int PageSize = 20,
    long EjercicioId = 0,
    long? SucursalId = null,
    string? Estado = null
) : IRequest<PagedResult<AsientoDto>>;