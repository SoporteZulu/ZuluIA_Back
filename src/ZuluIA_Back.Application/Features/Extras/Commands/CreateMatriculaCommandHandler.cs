using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class CreateMatriculaCommandHandler(
    IRepository<Matricula> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateMatriculaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateMatriculaCommand request, CancellationToken ct)
    {
        var nro = request.NroMatricula.Trim();
        var exists = await repo.ExistsAsync(
            x => x.SucursalId == request.SucursalId && x.NroMatricula == nro && x.DeletedAt == null,
            ct);
        if (exists)
            return Result.Failure<long>("Ya existe una matricula con ese numero para la sucursal.");

        Matricula entity;
        try
        {
            entity = Matricula.Crear(
                request.TerceroId,
                request.SucursalId,
                nro,
                request.Descripcion,
                request.FechaAlta,
                request.FechaVencimiento,
                userId: null);
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
