using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Retenciones.Commands;

public class DeleteTipoRetencionCommandHandler(
    IApplicationDbContext db,
    IUnitOfWork uow)
    : IRequestHandler<DeleteTipoRetencionCommand, Result>
{
    public async Task<Result> Handle(DeleteTipoRetencionCommand request, CancellationToken ct)
    {
        var tipo = await db.TiposRetencion
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (tipo is null)
            return Result.Failure($"No se encontró el tipo de retención con ID {request.Id}.");

        tipo.Dar_De_Baja(userId: null);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
