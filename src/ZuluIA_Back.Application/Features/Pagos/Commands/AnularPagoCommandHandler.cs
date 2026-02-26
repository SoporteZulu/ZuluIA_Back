using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Pagos.Commands;

public class AnularPagoCommandHandler(
    IRepository<Pago> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AnularPagoCommand, Result>
{
    public async Task<Result> Handle(AnularPagoCommand request, CancellationToken ct)
    {
        var pago = await repo.GetByIdAsync(request.Id, ct);

        if (pago is null)
            return Result.Failure($"No se encontró el pago con ID {request.Id}.");

        try
        {
            pago.Anular(currentUser.UserId);
            repo.Update(pago);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}