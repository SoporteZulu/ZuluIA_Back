using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.DescuentosComerciales.Commands;

public record UpdateDescuentoComercialCommand(
    long Id,
    DateOnly FechaDesde,
    DateOnly? FechaHasta,
    decimal Porcentaje
) : IRequest<Result>;
