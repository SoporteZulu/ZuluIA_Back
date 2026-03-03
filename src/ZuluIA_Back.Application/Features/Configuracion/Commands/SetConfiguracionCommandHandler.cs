using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Configuracion.Commands;

public class SetConfiguracionCommandHandler(IConfiguracionRepository repo)
    : IRequestHandler<SetConfiguracionCommand, Result>
{
    public async Task<Result> Handle(SetConfiguracionCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Campo))
            return Result.Failure("El campo no puede estar vacío.");

        await repo.UpsertAsync(request.Campo, request.Valor, ct);
        return Result.Success();
    }
}