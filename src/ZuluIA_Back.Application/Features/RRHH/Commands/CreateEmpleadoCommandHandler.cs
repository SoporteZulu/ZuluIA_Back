using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class CreateEmpleadoCommandHandler(
    IApplicationDbContext db,
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

        if (!await db.Terceros.AsNoTracking().AnyAsync(x => x.Id == request.TerceroId && !x.IsDeleted, ct))
            return Result.Failure<long>($"No se encontró el tercero ID {request.TerceroId}.");
        if (!await db.Sucursales.AsNoTracking().AnyAsync(x => x.Id == request.SucursalId, ct))
            return Result.Failure<long>($"No se encontró la sucursal ID {request.SucursalId}.");
        if (!await db.Monedas.AsNoTracking().AnyAsync(x => x.Id == request.MonedaId, ct))
            return Result.Failure<long>($"No se encontró la moneda ID {request.MonedaId}.");

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