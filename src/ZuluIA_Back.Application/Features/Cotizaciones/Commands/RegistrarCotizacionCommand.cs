using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Cotizaciones.Commands;

public record RegistrarCotizacionCommand(
    long MonedaId,
    DateOnly Fecha,
    decimal Cotizacion
) : IRequest<Result<long>>;