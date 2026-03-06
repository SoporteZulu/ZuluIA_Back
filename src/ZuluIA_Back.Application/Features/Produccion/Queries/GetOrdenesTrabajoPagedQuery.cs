using MediatR;
using ZuluIA_Back.Application.Features.Produccion.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Produccion.Queries;

public record GetOrdenesTrabajoPagedQuery(
    int Page,
    int PageSize,
    long? SucursalId,
    long? FormulaId,
    EstadoOrdenTrabajo? Estado,
    DateOnly? Desde,
    DateOnly? Hasta)
    : IRequest<PagedResult<OrdenTrabajoDto>>;