using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class DeleteBancoCommandHandler(
    IRepository<Banco> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeleteBancoCommand, Result>
{
    public async Task<Result> Handle(DeleteBancoCommand request, CancellationToken ct)
    {
        var banco = await repo.GetByIdAsync(request.Id, ct);
        if (banco is null)
            return Result.Failure($"Banco {request.Id} no encontrado.");

        repo.Remove(banco);

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Result.Failure("No se puede eliminar el banco porque tiene registros relacionados.");
        }

        return Result.Success();
    }
}
