using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

/// <summary>
/// Elimina un ítem de una lista de precios.
/// </summary>
public record RemoveItemDeListaCommand(long ListaId, long ItemId) : IRequest<Result>;