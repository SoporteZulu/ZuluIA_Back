using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.RetencionesPorPersona.Commands;

public class AsignarRetencionAPersonaCommandHandler(
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AsignarRetencionAPersonaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        AsignarRetencionAPersonaCommand request,
        CancellationToken ct)
    {
        var link = RetencionXPersona.Crear(
            request.TerceroId,
            request.TipoRetencionId,
            request.Descripcion,
            currentUser.UserId);

        db.RetencionesPorPersona.Add(link);
        await uow.SaveChangesAsync(ct);
        return Result.Success(link.Id);
    }
}

public class QuitarRetencionDePersonaCommandHandler(
    IRepository<RetencionXPersona> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<QuitarRetencionDePersonaCommand, Result>
{
    public async Task<Result> Handle(QuitarRetencionDePersonaCommand request, CancellationToken ct)
    {
        var link = await repo.GetByIdAsync(request.Id, ct);
        if (link is null)
            return Result.Failure($"No se encontró la asignación con ID {request.Id}.");

        link.Eliminar(currentUser.UserId);
        repo.Update(link);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
