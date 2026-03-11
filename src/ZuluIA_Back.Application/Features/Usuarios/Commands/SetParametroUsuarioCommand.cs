using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public record SetParametroUsuarioCommand(
    long UsuarioId,
    string Clave,
    string? Valor
) : IRequest<Result>;