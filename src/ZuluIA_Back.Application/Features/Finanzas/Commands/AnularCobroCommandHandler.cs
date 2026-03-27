using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class AnularCobroCommandHandler(
    ICobroRepository repo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AnularCobroCommand, Result>
{
    public async Task<Result> Handle(AnularCobroCommand request, CancellationToken ct)
    {
        var cobro = await repo.GetByIdAsync(request.Id, ct);
        if (cobro is null)
            return Result.Failure($"No se encontró el cobro con ID {request.Id}.");

        cobro.Anular(currentUser.UserId);
        repo.Update(cobro);

        var movimientos = await db.TesoreriaMovimientos
            .Where(x => x.ReferenciaTipo == "COBRO" && x.ReferenciaId == cobro.Id && !x.Anulado)
            .ToListAsync(ct);

        foreach (var movimiento in movimientos)
            movimiento.Anular(currentUser.UserId);

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}