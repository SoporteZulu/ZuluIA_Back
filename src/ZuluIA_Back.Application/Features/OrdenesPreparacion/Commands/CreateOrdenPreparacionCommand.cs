using MediatR;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public record CreateOrdenPreparacionCommand(
    long SucursalId,
    long? ComprobanteOrigenId,
    long? TerceroId,
    DateOnly Fecha,
    string? Observacion,
    IReadOnlyList<CreateOrdenPreparacionDetalleDto> Detalles
) : IRequest<Result<long>>;
