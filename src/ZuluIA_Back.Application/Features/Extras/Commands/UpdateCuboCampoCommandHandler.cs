using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class UpdateCuboCampoCommandHandler(
    IRepository<CuboCampo> campoRepo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateCuboCampoCommand, Result>
{
    public async Task<Result> Handle(UpdateCuboCampoCommand request, CancellationToken ct)
    {
        var campo = await campoRepo.FirstOrDefaultAsync(c => c.Id == request.CampoId && c.CuboId == request.CuboId, ct);
        if (campo is null)
            return Result.Failure("Campo no encontrado.");

        campo.Actualizar(
            request.Descripcion,
            request.Ubicacion,
            request.Posicion,
            request.Visible ?? true,
            request.Calculado ?? false,
            request.Filtro,
            request.CampoPadreId,
            request.Orden ?? 0,
            request.TipoOrden,
            request.FuncionAgregado);

        campoRepo.Update(campo);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class DeleteCuboCampoCommandHandler(
    IRepository<CuboCampo> campoRepo,
    IUnitOfWork uow)
    : IRequestHandler<DeleteCuboCampoCommand, Result>
{
    public async Task<Result> Handle(DeleteCuboCampoCommand request, CancellationToken ct)
    {
        var campo = await campoRepo.FirstOrDefaultAsync(c => c.Id == request.CampoId && c.CuboId == request.CuboId, ct);
        if (campo is null)
            return Result.Failure("Campo no encontrado.");

        campoRepo.Remove(campo);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
