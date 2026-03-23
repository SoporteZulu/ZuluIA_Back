using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class UpdateAtributoCommandHandler(
    IRepository<Atributo> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateAtributoCommand, Result>
{
    public async Task<Result> Handle(UpdateAtributoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Atributo {request.Id} no encontrado.");

        try
        {
            entity.Actualizar(request.Descripcion, request.Tipo, request.Requerido);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}