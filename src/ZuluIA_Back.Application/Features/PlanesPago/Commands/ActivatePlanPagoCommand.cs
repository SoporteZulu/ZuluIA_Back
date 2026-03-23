using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.PlanesPago.Commands;

public record ActivatePlanPagoCommand(long Id) : IRequest<Result>;
