using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public class CreateEjercicioCommandHandler(
    IEjercicioRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateEjercicioCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateEjercicioCommand request,
        CancellationToken ct)
    {
        var ejercicio = Ejercicio.Crear(
            request.Descripcion,
            request.FechaInicio,
            request.FechaFin,
            request.Mascara);

        foreach (var sucursalId in request.SucursalIds)
            ejercicio.AsignarSucursal(sucursalId);

        await repo.AddAsync(ejercicio, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(ejercicio.Id);
    }
}