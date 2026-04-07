using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class AddCuboFiltroCommandHandler(
    IRepository<Cubo> cuboRepo,
    IRepository<CuboFiltro> filtroRepo,
    IUnitOfWork uow)
    : IRequestHandler<AddCuboFiltroCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddCuboFiltroCommand request, CancellationToken ct)
    {
        var cuboExists = await cuboRepo.ExistsAsync(c => c.Id == request.CuboId, ct);
        if (!cuboExists)
            return Result.Failure<long>($"Cubo {request.CuboId} no encontrado.");

        CuboFiltro filtro;
        try
        {
            filtro = CuboFiltro.Crear(request.CuboId, request.Filtro, request.Operador ?? 1, request.Orden ?? 0);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        await filtroRepo.AddAsync(filtro, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(filtro.Id);
    }
}

public class UpdateCuboFiltroCommandHandler(
    IRepository<CuboFiltro> filtroRepo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateCuboFiltroCommand, Result>
{
    public async Task<Result> Handle(UpdateCuboFiltroCommand request, CancellationToken ct)
    {
        var filtro = await filtroRepo.FirstOrDefaultAsync(f => f.Id == request.FiltroId && f.CuboId == request.CuboId, ct);
        if (filtro is null)
            return Result.Failure("Filtro no encontrado.");

        try
        {
            filtro.Actualizar(request.Filtro, request.Operador ?? 1, request.Orden ?? 0);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        filtroRepo.Update(filtro);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class DeleteCuboFiltroCommandHandler(
    IRepository<CuboFiltro> filtroRepo,
    IUnitOfWork uow)
    : IRequestHandler<DeleteCuboFiltroCommand, Result>
{
    public async Task<Result> Handle(DeleteCuboFiltroCommand request, CancellationToken ct)
    {
        var filtro = await filtroRepo.FirstOrDefaultAsync(f => f.Id == request.FiltroId && f.CuboId == request.CuboId, ct);
        if (filtro is null)
            return Result.Failure("Filtro no encontrado.");

        filtroRepo.Remove(filtro);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
