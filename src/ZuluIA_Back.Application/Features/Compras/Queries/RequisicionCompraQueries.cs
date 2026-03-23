using MediatR;
using ZuluIA_Back.Application.Features.Compras.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Compras.Queries;

public record GetRequisicionesCompraPagedQuery(
    int Page, int PageSize,
    long? SucursalId, long? SolicitanteId, string? Estado)
    : IRequest<PagedResult<RequisicionCompraListDto>>;

public record GetRequisicionCompraDetalleQuery(long Id) : IRequest<RequisicionCompraDto?>;
