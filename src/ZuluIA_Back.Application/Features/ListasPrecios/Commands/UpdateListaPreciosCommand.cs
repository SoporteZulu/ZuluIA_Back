using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

public record UpdateListaPreciosCommand(
    long Id,
    string Descripcion,
    long MonedaId,
    DateOnly? VigenciaDesde,
    DateOnly? VigenciaHasta
) : IRequest<Result>;