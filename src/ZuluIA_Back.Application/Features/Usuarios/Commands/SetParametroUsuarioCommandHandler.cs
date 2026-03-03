using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class SetParametroUsuarioCommandHandler(IParametroUsuarioRepository repo)
    : IRequestHandler<SetParametroUsuarioCommand, Result>
{
    public async Task<Result> Handle(SetParametroUsuarioCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Clave))
            return Result.Failure("La clave no puede estar vacía.");

        await repo.UpsertAsync(request.UsuarioId, request.Clave, request.Valor, ct);
        return Result.Success();
    }
}