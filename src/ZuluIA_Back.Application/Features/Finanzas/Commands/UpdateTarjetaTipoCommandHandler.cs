using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class UpdateTarjetaTipoCommandHandler(
    IRepository<TarjetaTipo> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateTarjetaTipoCommand, Result>
{
    public async Task<Result> Handle(UpdateTarjetaTipoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Tarjeta {request.Id} no encontrada.");

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await repo.ExistsAsync(x => x.Id != request.Id && x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure("Ya existe una tarjeta con ese codigo.");

        try
        {
            entity.Actualizar(request.Descripcion, request.EsDebito, userId: null);
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
