using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record UpdateCategoriaItemCommand(
    long Id,
    string Codigo,
    string Descripcion,
    string? OrdenNivel) : IRequest<Result>;

public record DeleteCategoriaItemCommand(long Id) : IRequest<Result>;

public record ActivateCategoriaItemCommand(long Id) : IRequest<Result>;

public class UpdateCategoriaItemCommandHandler(
    ICategoriaItemRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateCategoriaItemCommand, Result>
{
    public async Task<Result> Handle(UpdateCategoriaItemCommand request, CancellationToken ct)
    {
        var categoria = await repo.GetByIdAsync(request.Id, ct);
        if (categoria is null)
            return Result.Failure($"No se encontró la categoría con ID {request.Id}.");

        if (await repo.ExisteCodigoAsync(request.Codigo, request.Id, ct))
            return Result.Failure($"Ya existe una categoría con el código '{request.Codigo}'.");

        categoria.Actualizar(request.Codigo, request.Descripcion, request.OrdenNivel, currentUser.UserId);
        repo.Update(categoria);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class DeleteCategoriaItemCommandHandler(
    ICategoriaItemRepository repo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteCategoriaItemCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoriaItemCommand request, CancellationToken ct)
    {
        var categoria = await repo.GetByIdAsync(request.Id, ct);
        if (categoria is null)
            return Result.Failure($"No se encontró la categoría con ID {request.Id}.");

        var tieneItems = await db.Items
            .AnyAsync(x => x.CategoriaId == request.Id && x.Activo, ct);

        if (tieneItems)
            return Result.Failure("No se puede desactivar una categoría que tiene ítems activos asociados.");

        categoria.Desactivar(currentUser.UserId);
        repo.Update(categoria);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class ActivateCategoriaItemCommandHandler(
    ICategoriaItemRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<ActivateCategoriaItemCommand, Result>
{
    public async Task<Result> Handle(ActivateCategoriaItemCommand request, CancellationToken ct)
    {
        var categoria = await repo.GetByIdAsync(request.Id, ct);
        if (categoria is null)
            return Result.Failure($"No se encontró la categoría con ID {request.Id}.");

        categoria.Activar(currentUser.UserId);
        repo.Update(categoria);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
