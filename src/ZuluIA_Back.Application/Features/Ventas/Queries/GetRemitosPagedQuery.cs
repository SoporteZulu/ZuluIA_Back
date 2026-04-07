using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Ventas.Queries;

public record GetRemitosPagedQuery(
    int Page = 1,
    int PageSize = 20,
    long? SucursalId = null,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null,
    short? Prefijo = null,
    long? Numero = null,
    string? TerceroLegajo = null,
    string? TerceroDenominacionSocial = null,
    string? CotNumero = null,
    DateOnly? CotFechaDesde = null,
    DateOnly? CotFechaHasta = null,
    long? DepositoId = null,
    EstadoComprobante? Estado = null,
    EstadoLogisticoRemito? EstadoLogistico = null,
    bool? EsValorizado = null) : IRequest<PagedResult<ComprobanteListDto>>;
