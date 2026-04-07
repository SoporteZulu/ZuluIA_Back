using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class ActivatePuntoFacturacionCommandHandler(
    IPuntoFacturacionRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<ActivatePuntoFacturacionCommand, Result>
{
    public async Task<Result> Handle(ActivatePuntoFacturacionCommand request, CancellationToken ct)
    {
        var punto = await repo.GetByIdAsync(request.Id, ct);

        if (punto is null)
            return Result.Failure($"No se encontro el punto de facturacion con ID {request.Id}.");

        punto.Activar(currentUser.UserId);
        repo.Update(punto);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}