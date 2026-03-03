using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.PlanesPago.Commands;

public record CreatePlanPagoCommand(
    string Descripcion,
    short CantidadCuotas,
    decimal InteresPct
) : IRequest<Result<long>>;