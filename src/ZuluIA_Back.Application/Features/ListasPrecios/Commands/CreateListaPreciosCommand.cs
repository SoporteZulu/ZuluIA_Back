using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

public record CreateListaPreciosCommand(
    string Descripcion,
    long MonedaId,
    DateOnly? VigenciaDesde,
    DateOnly? VigenciaHasta
) : IRequest<Result<long>>;