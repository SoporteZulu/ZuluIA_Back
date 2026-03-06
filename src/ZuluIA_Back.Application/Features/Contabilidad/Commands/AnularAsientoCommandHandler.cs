using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public class AnularAsientoCommandHandler(
    IAsientoRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<AnularAsientoCommand, Result>
{
    public async Task<Result> Handle(AnularAsientoCommand request, CancellationToken ct)
    {
        var asiento = await repo.GetByIdAsync(request.Id, ct);
        if (asiento is null)
            return Result.Failure($"No se encontró el asiento con ID {request.Id}.");

        asiento.Anular();
        repo.Update(asiento);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}