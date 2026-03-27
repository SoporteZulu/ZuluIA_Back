using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class CreatePlanTarjetaCommandHandler(
    IRepository<PlanTarjeta> repo,
    IRepository<TarjetaTipo> tarjetaTipoRepo,
    IUnitOfWork uow)
    : IRequestHandler<CreatePlanTarjetaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreatePlanTarjetaCommand request, CancellationToken ct)
    {
        var tarjetaExists = await tarjetaTipoRepo.ExistsAsync(x => x.Id == request.TarjetaTipoId, ct);
        if (!tarjetaExists)
            return Result.Failure<long>("No existe la tarjeta indicada.");

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var duplicate = await repo.ExistsAsync(
            x => x.TarjetaTipoId == request.TarjetaTipoId && x.Codigo == codigo,
            ct);
        if (duplicate)
            return Result.Failure<long>("Ya existe un plan con ese codigo para esa tarjeta.");

        PlanTarjeta entity;
        try
        {
            entity = PlanTarjeta.Crear(
                request.TarjetaTipoId,
                codigo,
                request.Descripcion,
                request.CantidadCuotas,
                request.Recargo,
                request.DiasAcreditacion,
                userId: null);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Failure<long>(ex.Message);
        }

        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(entity.Id);
    }
}
