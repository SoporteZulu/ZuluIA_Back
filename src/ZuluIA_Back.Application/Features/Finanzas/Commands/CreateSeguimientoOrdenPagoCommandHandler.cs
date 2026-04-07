using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class CreateSeguimientoOrdenPagoCommandHandler(
    IRepository<SeguimientoOrdenPago> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateSeguimientoOrdenPagoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateSeguimientoOrdenPagoCommand request, CancellationToken ct)
    {
        SeguimientoOrdenPago entity;
        try
        {
            entity = SeguimientoOrdenPago.Registrar(
                request.PagoId,
                request.SucursalId,
                request.Fecha,
                request.Estado,
                request.Observacion,
                request.UsuarioId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(entity.Id);
    }
}

public class UpdateSeguimientoOrdenPagoObservacionCommandHandler(
    IRepository<SeguimientoOrdenPago> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateSeguimientoOrdenPagoObservacionCommand, Result<UpdateSeguimientoOrdenPagoObservacionResult>>
{
    public async Task<Result<UpdateSeguimientoOrdenPagoObservacionResult>> Handle(UpdateSeguimientoOrdenPagoObservacionCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure<UpdateSeguimientoOrdenPagoObservacionResult>($"Seguimiento {request.Id} no encontrado.");

        entity.ActualizarObservacion(request.Observacion, request.UsuarioId);
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success(new UpdateSeguimientoOrdenPagoObservacionResult(entity.Id, entity.Observacion));
    }
}
