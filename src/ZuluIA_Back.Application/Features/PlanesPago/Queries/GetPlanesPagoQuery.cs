using MediatR;
using ZuluIA_Back.Application.Features.PlanesPago.DTOs;

namespace ZuluIA_Back.Application.Features.PlanesPago.Queries;

public record GetPlanesPagoQuery(bool SoloActivos = true) : IRequest<IReadOnlyList<PlanPagoDto>>;