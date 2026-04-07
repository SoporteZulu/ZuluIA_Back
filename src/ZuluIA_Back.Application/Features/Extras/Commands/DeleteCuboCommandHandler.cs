using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class DeleteCuboCommandHandler(
    IRepository<Cubo> cuboRepo,
    IRepository<CuboCampo> campoRepo,
    IRepository<CuboFiltro> filtroRepo,
    IUnitOfWork uow)
    : IRequestHandler<DeleteCuboCommand, Result>
{
    public async Task<Result> Handle(DeleteCuboCommand request, CancellationToken ct)
    {
        var cubo = await cuboRepo.GetByIdAsync(request.Id, ct);
        if (cubo is null)
            return Result.Failure($"Cubo {request.Id} no encontrado.");

        var campos = await campoRepo.FindAsync(c => c.CuboId == request.Id, ct);
        var filtros = await filtroRepo.FindAsync(f => f.CuboId == request.Id, ct);

        foreach (var campo in campos)
            campoRepo.Remove(campo);

        foreach (var filtro in filtros)
            filtroRepo.Remove(filtro);

        cuboRepo.Remove(cubo);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
