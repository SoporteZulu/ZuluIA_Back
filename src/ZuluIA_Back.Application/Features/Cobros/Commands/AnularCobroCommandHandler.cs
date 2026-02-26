using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cobros.Commands;

public class AnularCobroCommandHandler(
    IRepository<Cobro> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AnularCobroCommand, Result>
{
    public async Task<Result> Handle(AnularCobroCommand request, CancellationToken ct)
    {
        var cobro = await repo.GetByIdAsync(request.Id, ct);

        if (cobro is null)
            return Result.Failure($"No se encontró el cobro con ID {request.Id}.");

        try
        {
            cobro.Anular(currentUser.UserId);
            repo.Update(cobro);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}