using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Cotizaciones.Commands;

public record ImportarCotizacionItemInput(
    DateOnly Fecha,
    decimal Cotizacion);

public record ImportarCotizacionesCommand(
    long MonedaId,
    IReadOnlyList<ImportarCotizacionItemInput> Items) : IRequest<Result<ImportarCotizacionesResultDto>>;

public record ImportarCotizacionesResultDto(
    long MonedaId,
    int Procesadas,
    int Creadas,
    int Actualizadas);