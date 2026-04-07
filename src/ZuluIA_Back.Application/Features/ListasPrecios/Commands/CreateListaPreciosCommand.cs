using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

public record CreateListaPreciosCommand(
    string Descripcion,
    long MonedaId,
    DateOnly? VigenciaDesde,
    DateOnly? VigenciaHasta,
    bool EsPorDefecto = false,
    long? ListaPadreId = null,
    int Prioridad = 0,
    string? Observaciones = null
) : IRequest<Result<long>>;