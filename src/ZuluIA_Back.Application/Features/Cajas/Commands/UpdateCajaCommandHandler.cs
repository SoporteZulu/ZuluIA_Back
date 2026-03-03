using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public class UpdateCajaCommandHandler(
    ICajaRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateCajaCommand, Result>
{
    public async Task<Result> Handle(UpdateCajaCommand request, CancellationToken ct)
    {
        var caja = await repo.GetByIdAsync(request.Id, ct);
        if (caja is null)
            return Result.Failure($"No se encontró la caja con ID {request.Id}.");

        caja.Actualizar(
            request.Descripcion,
            request.TipoId,
            request.MonedaId,
            request.Banco,
            request.NroCuenta,
            request.Cbu,
            request.UsuarioId,
            request.EsCaja,
            currentUser.UserId);

        repo.Update(caja);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}