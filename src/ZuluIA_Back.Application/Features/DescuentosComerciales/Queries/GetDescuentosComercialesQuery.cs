using MediatR;
using ZuluIA_Back.Application.Features.DescuentosComerciales.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.DescuentosComerciales.Queries;

/// <summary>Retorna descuentos comerciales filtrando por tercero y/o ítem.</summary>
public record GetDescuentosComercialesQuery(
    long? TerceroId,
    long? ItemId,
    DateOnly? VigenteEn = null
) : IRequest<IReadOnlyList<DescuentoComercialDto>>;
