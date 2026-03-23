using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class UpdateBancoCommandHandler(
    IRepository<Banco> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateBancoCommand, Result<UpdateBancoResult>>
{
    public async Task<Result<UpdateBancoResult>> Handle(UpdateBancoCommand request, CancellationToken ct)
    {
        var banco = await repo.GetByIdAsync(request.Id, ct);
        if (banco is null)
            return Result.Failure<UpdateBancoResult>($"Banco {request.Id} no encontrado.");

        var normalized = request.Descripcion.Trim();
        var exists = await repo.ExistsAsync(x => x.Id != request.Id && x.Descripcion == normalized, ct);
        if (exists)
            return Result.Failure<UpdateBancoResult>("Ya existe un banco con esa descripcion.");

        try
        {
            banco.Actualizar(normalized);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<UpdateBancoResult>(ex.Message);
        }

        repo.Update(banco);
        await uow.SaveChangesAsync(ct);

        return Result.Success(new UpdateBancoResult(banco.Id, banco.Descripcion));
    }
}
