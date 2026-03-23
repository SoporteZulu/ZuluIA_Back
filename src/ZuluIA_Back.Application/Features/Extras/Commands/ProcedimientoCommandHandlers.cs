using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class CreateProcedimientoCommandHandler(
    IRepository<Busqueda> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateProcedimientoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateProcedimientoCommand request, CancellationToken ct)
    {
        Busqueda entity;
        try
        {
            entity = Busqueda.Crear(
                request.Nombre,
                "procedimientos",
                request.DefinicionJson,
                request.UsuarioId,
                request.EsGlobal);
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

public class UpdateProcedimientoCommandHandler(
    IRepository<Busqueda> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateProcedimientoCommand, Result>
{
    public async Task<Result> Handle(UpdateProcedimientoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null || !string.Equals(entity.Modulo, "procedimientos", StringComparison.OrdinalIgnoreCase))
            return Result.Failure($"Procedimiento {request.Id} no encontrado.");

        try
        {
            entity.Actualizar(request.Nombre, request.DefinicionJson, request.EsGlobal);
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

public class DeleteProcedimientoCommandHandler(
    IRepository<Busqueda> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeleteProcedimientoCommand, Result>
{
    public async Task<Result> Handle(DeleteProcedimientoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null || !string.Equals(entity.Modulo, "procedimientos", StringComparison.OrdinalIgnoreCase))
            return Result.Failure($"Procedimiento {request.Id} no encontrado.");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
