using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class CreateCuboCommandHandler(
    IRepository<Cubo> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateCuboCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCuboCommand request, CancellationToken ct)
    {
        Cubo cubo;
        try
        {
            cubo = Cubo.Crear(
                request.Descripcion,
                request.OrigenDatos,
                request.Observacion,
                request.AmbienteId ?? 1,
                request.MenuCuboId,
                request.CuboOrigenId,
                request.EsSistema ?? false,
                request.FormatoId,
                request.UsuarioAltaId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        await repo.AddAsync(cubo, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(cubo.Id);
    }
}
