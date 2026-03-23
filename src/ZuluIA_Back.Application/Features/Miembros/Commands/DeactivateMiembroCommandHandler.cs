using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Miembros.Commands;

public class DeactivateMiembroCommandHandler(
    ITerceroRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<DeactivateMiembroCommand, Result>
{
    public async Task<Result> Handle(DeactivateMiembroCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);

        if (entity is null || !entity.EsCliente)
            return Result.Failure($"Miembro {request.Id} no encontrado.");

        entity.Desactivar(currentUser.UserId);
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}