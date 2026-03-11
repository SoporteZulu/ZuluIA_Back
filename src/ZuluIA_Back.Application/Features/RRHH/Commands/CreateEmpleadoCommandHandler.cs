using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class CreateEmpleadoCommandHandler(
    IEmpleadoRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateEmpleadoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateEmpleadoCommand request,
        CancellationToken ct)
    {
        if (await repo.ExisteLegajoAsync(request.Legajo, null, ct))
            return Result.Failure<long>(
                $"Ya existe un empleado con legajo '{request.Legajo}'.");

        var empleado = Empleado.Crear(
            request.TerceroId,
            request.SucursalId,
            request.Legajo,
            request.Cargo,
            request.Area,
            request.FechaIngreso,
            request.SueldoBasico,
            request.MonedaId);

        await repo.AddAsync(empleado, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(empleado.Id);
    }
}