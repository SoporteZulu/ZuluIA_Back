using MediatR;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Cheques.Queries;

public record GetChequesPagedQuery(
    int Page,
    int PageSize,
    long? CajaId,
    long? TerceroId,
    EstadoCheque? Estado,
    TipoCheque? Tipo,
    bool? EsALaOrden,
    bool? EsCruzado,
    string? Banco,
    string? NroCheque,
    string? Titular,
    DateOnly? Desde,
    DateOnly? Hasta
) : IRequest<PagedResult<ChequeDto>>;