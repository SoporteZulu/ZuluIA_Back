using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class UpdateCuboCommandHandler(
    IRepository<Cubo> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateCuboCommand, Result>
{
    public async Task<Result> Handle(UpdateCuboCommand request, CancellationToken ct)
    {
        var cubo = await repo.GetByIdAsync(request.Id, ct);
        if (cubo is null)
            return Result.Failure($"Cubo {request.Id} no encontrado.");

        try
        {
            cubo.Actualizar(
                request.Descripcion,
                request.OrigenDatos,
                request.Observacion,
                request.AmbienteId ?? 1,
                request.MenuCuboId,
                request.FormatoId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        repo.Update(cubo);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
