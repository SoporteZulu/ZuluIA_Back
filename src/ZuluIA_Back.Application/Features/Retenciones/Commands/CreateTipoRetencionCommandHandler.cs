using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Retenciones.Commands;

public class CreateTipoRetencionCommandHandler(
    IApplicationDbContext db,
    IUnitOfWork uow)
    : IRequestHandler<CreateTipoRetencionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateTipoRetencionCommand request,
        CancellationToken ct)
    {
        var tipo = TipoRetencion.Crear(
            request.Descripcion,
            request.Regimen,
            request.MinimoNoImponible,
            request.AcumulaPago,
            request.TipoComprobanteId,
            request.ItemId,
            userId: null);

        foreach (var e in request.Escalas)
            tipo.AgregarEscala(e.Descripcion, e.ImporteDesde, e.ImporteHasta, e.Porcentaje);

        db.TiposRetencion.Add(tipo);
        await uow.SaveChangesAsync(ct);

        return Result.Success(tipo.Id);
    }
}
