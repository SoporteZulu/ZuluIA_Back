using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Recibos.Commands;

public class AnularReciboCommandHandler(
    IReciboRepository reciboRepo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AnularReciboCommand, Result>
{
    public async Task<Result> Handle(AnularReciboCommand request, CancellationToken ct)
    {
        var recibo = await reciboRepo.GetByIdAsync(request.Id, ct);
        if (recibo is null)
            return Result.Failure("Recibo no encontrado.");

        recibo.Anular(currentUser.UserId);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
