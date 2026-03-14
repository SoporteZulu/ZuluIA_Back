using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public class AnularOrdenPreparacionCommandHandler(
    IRepository<OrdenPreparacion> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AnularOrdenPreparacionCommand, Result>
{
    public async Task<Result> Handle(AnularOrdenPreparacionCommand request, CancellationToken ct)
    {
        var orden = await repo.GetByIdAsync(request.Id, ct);
        if (orden is null)
            return Result.Failure($"No se encontró la orden de preparación con ID {request.Id}.");

        try
        {
            orden.Anular(currentUser.UserId);
            repo.Update(orden);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
