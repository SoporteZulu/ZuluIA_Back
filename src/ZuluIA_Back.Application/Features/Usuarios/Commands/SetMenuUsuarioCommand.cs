using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

/// <summary>
/// Reemplaza completamente los ítems de menú asignados a un usuario.
/// </summary>
public record SetMenuUsuarioCommand(
    long UsuarioId,
    IReadOnlyList<long> MenuIds
) : IRequest<Result>;