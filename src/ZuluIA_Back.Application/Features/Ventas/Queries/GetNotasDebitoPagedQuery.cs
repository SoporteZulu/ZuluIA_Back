using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Ventas.Queries;

public record GetNotasDebitoPagedQuery(
    int Page = 1,
    int PageSize = 20,
    long? SucursalId = null,
    long? TerceroId = null,
    long? TipoComprobanteId = null,
    EstadoComprobante? Estado = null,
    DateOnly? Desde = null,
    DateOnly? Hasta = null,
    long? ComprobanteOrigenId = null,
    long? MotivoDebitoId = null)
    : IRequest<PagedResult<ComprobanteListDto>>;
