using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class DeleteTerceroCommandHandler(
    ITerceroRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteTerceroCommand, Result>
{
    public async Task<Result> Handle(DeleteTerceroCommand request, CancellationToken ct)
    {
        var tercero = await repo.GetByIdAsync(request.Id, ct);

        if (tercero is null)
            return Result.Failure($"No se encontró el tercero con ID {request.Id}.");

        tercero.Desactivar(currentUser.UserId);
        repo.Update(tercero);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}