using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public record CerrarEjercicioCommand(long Id) : IRequest<Result>;

public record ReabrirEjercicioCommand(long Id) : IRequest<Result>;

public record AsignarSucursalEjercicioCommand(
    long EjercicioId,
    long SucursalId,
    bool UsaContabilidad) : IRequest<Result>;

public class CerrarEjercicioCommandHandler(
    IEjercicioRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<CerrarEjercicioCommand, Result>
{
    public async Task<Result> Handle(CerrarEjercicioCommand request, CancellationToken ct)
    {
        var ejercicio = await repo.GetByIdAsync(request.Id, ct);
        if (ejercicio is null)
            return Result.Failure($"No se encontró el ejercicio con ID {request.Id}.");

        try
        {
            ejercicio.Cerrar();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        repo.Update(ejercicio);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class ReabrirEjercicioCommandHandler(
    IEjercicioRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<ReabrirEjercicioCommand, Result>
{
    public async Task<Result> Handle(ReabrirEjercicioCommand request, CancellationToken ct)
    {
        var ejercicio = await repo.GetByIdAsync(request.Id, ct);
        if (ejercicio is null)
            return Result.Failure($"No se encontró el ejercicio con ID {request.Id}.");

        try
        {
            ejercicio.Reabrir();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        repo.Update(ejercicio);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class AsignarSucursalEjercicioCommandHandler(
    IEjercicioRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<AsignarSucursalEjercicioCommand, Result>
{
    public async Task<Result> Handle(AsignarSucursalEjercicioCommand request, CancellationToken ct)
    {
        var ejercicio = await repo.GetByIdConSucursalesAsync(request.EjercicioId, ct);
        if (ejercicio is null)
            return Result.Failure($"No se encontró el ejercicio con ID {request.EjercicioId}.");

        ejercicio.AsignarSucursal(request.SucursalId, request.UsaContabilidad);
        repo.Update(ejercicio);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
