using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Application.Features.Cobros.DTOs;

namespace ZuluIA_Back.Application.Features.Cobros.Commands;

public record CreateCobroCommand(
    long SucursalId,
    long TerceroId,
    DateOnly Fecha,
    long MonedaId,
    decimal Cotizacion,
    string? Observacion,
    List<CreateCobroMedioDto> Medios
) : IRequest<Result<long>>;