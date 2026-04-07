using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Extensions;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Retenciones.Commands;

public class UpdateTipoRetencionCommandHandler(
    IApplicationDbContext db,
    IUnitOfWork uow)
    : IRequestHandler<UpdateTipoRetencionCommand, Result>
{
    public async Task<Result> Handle(
        UpdateTipoRetencionCommand request,
        CancellationToken ct)
    {
        var tipo = await db.TiposRetencion
            .AsQueryableSafe()
            .FirstOrDefaultSafeAsync(x => x.Id == request.Id, ct);

        if (tipo is null)
            return Result.Failure($"No se encontró el tipo de retención con ID {request.Id}.");

        tipo.Actualizar(
            request.Descripcion,
            request.Regimen,
            request.MinimoNoImponible,
            request.AcumulaPago,
            request.TipoComprobanteId,
            request.ItemId,
            userId: null);

        // Reemplazar escalas completamente
        tipo.RemoverEscalas();

        foreach (var e in request.Escalas)
            tipo.AgregarEscala(e.Descripcion, e.ImporteDesde, e.ImporteHasta, e.Porcentaje);

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
