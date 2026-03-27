using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public class UpdateCentroCostoCommandHandler(
    IRepository<CentroCosto> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateCentroCostoCommand, Result>
{
    public async Task<Result> Handle(UpdateCentroCostoCommand request, CancellationToken ct)
    {
        var cc = await repo.GetByIdAsync(request.Id, ct);
        if (cc is null)
            return Result.Failure($"No se encontro el centro de costo con ID {request.Id}.");

        try
        {
            cc.Actualizar(request.Descripcion);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        repo.Update(cc);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}