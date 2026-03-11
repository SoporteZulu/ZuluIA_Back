using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public record SetPermisoUsuarioCommand(
    long UsuarioId,
    long SeguridadId,
    bool Valor
) : IRequest<Result>;