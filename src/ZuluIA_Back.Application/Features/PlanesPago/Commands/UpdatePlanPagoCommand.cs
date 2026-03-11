using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.PlanesPago.Commands;

public record UpdatePlanPagoCommand(
    long Id,
    string Descripcion,
    short CantidadCuotas,
    decimal InteresPct
) : IRequest<Result>;