using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record UpdateDepositoCommand(long Id, string Descripcion, bool EsDefault) : IRequest<Result>;

public record DeleteDepositoCommand(long Id) : IRequest<Result>;

public record ActivateDepositoCommand(long Id) : IRequest<Result>;

public class UpdateDepositoCommandHandler(
    IDepositoRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateDepositoCommand, Result>
{
    public async Task<Result> Handle(UpdateDepositoCommand request, CancellationToken ct)
    {
        var deposito = await repo.GetByIdAsync(request.Id, ct);
        if (deposito is null)
            return Result.Failure($"No se encontró el depósito con ID {request.Id}.");

        if (request.EsDefault && !deposito.EsDefault)
        {
            var defaultActual = await repo.GetDefaultBySucursalAsync(deposito.SucursalId, ct);
            if (defaultActual is not null && defaultActual.Id != deposito.Id)
            {
                defaultActual.UnsetDefault();
                repo.Update(defaultActual);
            }
        }

        deposito.Actualizar(request.Descripcion, request.EsDefault);
        repo.Update(deposito);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class DeleteDepositoCommandHandler(
    IDepositoRepository repo,
    IApplicationDbContext db,
    IUnitOfWork uow)
    : IRequestHandler<DeleteDepositoCommand, Result>
{
    public async Task<Result> Handle(DeleteDepositoCommand request, CancellationToken ct)
    {
        var deposito = await repo.GetByIdAsync(request.Id, ct);
        if (deposito is null)
            return Result.Failure($"No se encontró el depósito con ID {request.Id}.");

        if (deposito.EsDefault)
            return Result.Failure("No se puede desactivar el depósito por defecto. Asigne otro depósito como default primero.");

        var tieneStock = await db.Stock
            .AnyAsync(x => x.DepositoId == request.Id && x.Cantidad > 0, ct);

        if (tieneStock)
            return Result.Failure("No se puede desactivar un depósito que tiene stock disponible.");

        deposito.Desactivar();
        repo.Update(deposito);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class ActivateDepositoCommandHandler(
    IDepositoRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<ActivateDepositoCommand, Result>
{
    public async Task<Result> Handle(ActivateDepositoCommand request, CancellationToken ct)
    {
        var deposito = await repo.GetByIdAsync(request.Id, ct);
        if (deposito is null)
            return Result.Failure($"No se encontró el depósito con ID {request.Id}.");

        deposito.Activar();
        repo.Update(deposito);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
