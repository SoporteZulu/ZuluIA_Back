using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class CreateBusquedaCommandHandler(
    IRepository<Busqueda> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateBusquedaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateBusquedaCommand request, CancellationToken ct)
    {
        Busqueda busqueda;
        try
        {
            busqueda = Busqueda.Crear(
                request.Nombre,
                request.Modulo,
                request.FiltrosJson,
                request.UsuarioId,
                request.EsGlobal);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        await repo.AddAsync(busqueda, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(busqueda.Id);
    }
}

public class UpdateBusquedaCommandHandler(
    IRepository<Busqueda> repo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateBusquedaCommand, Result>
{
    public async Task<Result> Handle(UpdateBusquedaCommand request, CancellationToken ct)
    {
        var busqueda = await repo.GetByIdAsync(request.Id, ct);
        if (busqueda is null)
            return Result.Failure($"No se encontro la busqueda con ID {request.Id}.");

        try
        {
            busqueda.Actualizar(request.Nombre, request.FiltrosJson, request.EsGlobal);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        repo.Update(busqueda);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class DeleteBusquedaCommandHandler(
    IRepository<Busqueda> repo,
    IUnitOfWork uow)
    : IRequestHandler<DeleteBusquedaCommand, Result>
{
    public async Task<Result> Handle(DeleteBusquedaCommand request, CancellationToken ct)
    {
        var busqueda = await repo.GetByIdAsync(request.Id, ct);
        if (busqueda is null)
            return Result.Failure($"No se encontro la busqueda con ID {request.Id}.");

        repo.Remove(busqueda);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
