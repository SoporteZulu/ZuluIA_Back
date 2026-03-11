using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public class RegistrarAsientoCommandHandler(
    ContabilidadService contabilidadService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<RegistrarAsientoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        RegistrarAsientoCommand request,
        CancellationToken ct)
    {
        var lineas = request.Lineas
            .Select((l, i) => (
                l.CuentaId,
                l.Debe,
                l.Haber,
                l.Descripcion,
                l.CentroCostoId))
            .ToList();

        var asiento = await contabilidadService.RegistrarAsientoAsync(
            request.EjercicioId,
            request.SucursalId,
            request.Fecha,
            request.Descripcion,
            request.OrigenTabla,
            request.OrigenId,
            lineas,
            currentUser.UserId,
            ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success(asiento.Id);
    }
}