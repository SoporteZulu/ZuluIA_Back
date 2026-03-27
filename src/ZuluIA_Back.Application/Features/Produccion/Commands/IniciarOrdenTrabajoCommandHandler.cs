using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public class IniciarOrdenTrabajoCommandHandler(
    IOrdenTrabajoRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<IniciarOrdenTrabajoCommand, Result>
{
    public async Task<Result> Handle(IniciarOrdenTrabajoCommand request, CancellationToken ct)
    {
        var ot = await repo.GetByIdAsync(request.Id, ct);
        if (ot is null)
            return Result.Failure($"No se encontró la OT con ID {request.Id}.");

        try
        {
            ot.Iniciar(currentUser.UserId);
            repo.Update(ot);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
