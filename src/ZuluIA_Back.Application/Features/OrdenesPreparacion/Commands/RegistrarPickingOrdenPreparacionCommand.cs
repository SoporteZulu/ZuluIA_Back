using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public record RegistrarPickingDetalleInput(long DetalleId, decimal CantidadEntregada);

public record RegistrarPickingOrdenPreparacionCommand(long Id, IReadOnlyList<RegistrarPickingDetalleInput> Detalles) : IRequest<Result>;
