using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Sucursales.Commands;

public class DeleteSucursalCommandHandler(
    ISucursalRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteSucursalCommand, Result>
{
    public async Task<Result> Handle(DeleteSucursalCommand request, CancellationToken ct)
    {
        var sucursal = await repo.GetByIdAsync(request.Id, ct);
        if (sucursal is null)
            return Result.Failure($"No se encontró la sucursal con ID {request.Id}.");

        sucursal.Desactivar(currentUser.UserId);
        repo.Update(sucursal);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}