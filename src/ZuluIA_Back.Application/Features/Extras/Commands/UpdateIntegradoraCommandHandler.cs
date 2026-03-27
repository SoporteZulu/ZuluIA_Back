using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class UpdateIntegradoraCommandHandler(
    IRepository<Integradora> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateIntegradoraCommand, Result>
{
    public async Task<Result> Handle(UpdateIntegradoraCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return Result.Failure($"Integradora {request.Id} no encontrada.");

        entity.Actualizar(request.Nombre, request.TipoSistema, request.UrlEndpoint, request.Configuracion, currentUser.UserId);
        repo.Update(entity);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}