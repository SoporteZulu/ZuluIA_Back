using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public record UpdateTimbradoFiscalCommand(
    long Id,
    string NumeroTimbrado,
    DateOnly VigenciaDesde,
    DateOnly VigenciaHasta,
    string? Observacion) : IRequest<Result>;
