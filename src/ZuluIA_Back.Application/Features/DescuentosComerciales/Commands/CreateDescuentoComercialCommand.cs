using MediatR;
using ZuluIA_Back.Application.Features.DescuentosComerciales.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.DescuentosComerciales.Commands;

public record CreateDescuentoComercialCommand(
    long TerceroId,
    long ItemId,
    DateOnly FechaDesde,
    DateOnly? FechaHasta,
    decimal Porcentaje
) : IRequest<Result<long>>;
