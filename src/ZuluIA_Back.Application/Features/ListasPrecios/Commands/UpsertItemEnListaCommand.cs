using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

/// <summary>
/// Agrega o actualiza el precio de un ítem dentro de una lista.
/// </summary>
public record UpsertItemEnListaCommand(
    long ListaId,
    long ItemId,
    decimal Precio,
    decimal DescuentoPct
) : IRequest<Result>;