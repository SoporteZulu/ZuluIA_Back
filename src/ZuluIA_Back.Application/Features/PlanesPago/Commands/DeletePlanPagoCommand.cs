using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.PlanesPago.Commands;

public record DeletePlanPagoCommand(long Id) : IRequest<Result>;